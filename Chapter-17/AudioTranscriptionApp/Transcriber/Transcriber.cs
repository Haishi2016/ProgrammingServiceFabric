using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Transcriber.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Bing.Speech;
using System.IO;
using System.Text;
using NAudio.Wave;

namespace Transcriber
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Transcriber : Actor, ITranscriber, IRemindable
    {
        private static readonly Task CompletedTask = Task.FromResult(true);
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Dictionary<int, string> mTexts = new Dictionary<int, string>();
        private WaveFormat mStandardFormat;
        private string mDukeFile;
        private IActorReminder mReminder;
        /// <summary>
        /// Initializes a new instance of Transcriber
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Transcriber(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            var dataPackage = this.ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Data");

            mDukeFile = Path.Combine(dataPackage.Path, "nukem-01.wav");

            using (WaveFileReader nukeReader = new WaveFileReader(mDukeFile))
            {
                mStandardFormat = nukeReader.WaveFormat;
            }

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public async Task SubmitJob(string url, bool isPublic, CancellationToken cancellationToken)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string fileName = "";
            if (!isPublic)
                fileName = await getAudioFileFromBlob(tempFolder, url);

            var files = await splitFiles(await convertToWav(fileName));

            await this.StateManager.TryAddStateAsync<string>("FileName", url.Replace('.','_'));
            await this.StateManager.TryAddStateAsync<List<string>>("Files", files);
            await this.StateManager.TryAddStateAsync<int>("FileIndex", -1);
            await this.StateManager.TryAddStateAsync<bool>("ActiveFile", false);
            await this.StateManager.SaveStateAsync();
            mReminder = await this.RegisterReminderAsync("Transcription", new byte[] { (byte)files.Count }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));            
        }
        private async Task<string> convertToWav(string file)
        {
            string fileWithouExtension = Path.Combine(Path.GetFullPath(file).Replace(Path.GetFileName(file), ""), Path.GetFileNameWithoutExtension(file));
            string outFile = file;
            string standard_outFile = fileWithouExtension + "_standard.wav";
            string tagged_outFile = fileWithouExtension + "_tagged.wav";
            if (file.ToLower().EndsWith(".mp3"))
            {
                outFile = fileWithouExtension + ".wav";

                using (Mp3FileReader reader = new Mp3FileReader(file))
                {
                    WaveFileWriter.CreateWaveFile(outFile, reader);
                }
            }
            using (WaveFileReader maleReader = new WaveFileReader(outFile))
            {
                using (var conversionStream = new WaveFormatConversionStream(mStandardFormat, maleReader))
                {
                    WaveFileWriter.CreateWaveFile(standard_outFile, conversionStream);
                }
            }
            List<string> sourceFiles = new List<string> { standard_outFile, mDukeFile };
            WaveFileWriter mergeWaveFileWriter = null;
            int read;
            byte[] readBuffer = new byte[1024];

            foreach (string sourceFile in sourceFiles)
            {
                using (WaveFileReader reader = new WaveFileReader(sourceFile))
                {
                    if (mergeWaveFileWriter == null)
                    {
                        //   first time in create new Writer
                        mergeWaveFileWriter = new WaveFileWriter(tagged_outFile, mStandardFormat);
                    }

                    while ((read = reader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        mergeWaveFileWriter.Write(readBuffer, 0, read);
                    }
                }
            }
            mergeWaveFileWriter.Dispose();

            return await Task.FromResult<string>(tagged_outFile);
        }
        private async Task<List<string>> splitFiles(string file)
        {
            return await Task.FromResult<List<string>>(new List<string> { file });
        }
        private async Task<string> getAudioFileFromBlob(string tempFolder, string url)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("clips");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(url);
            string fileName = Path.Combine(tempFolder, url);
            await blockBlob.DownloadToFileAsync(fileName, FileMode.Create);
            return fileName;
        }
        private async Task transcribeAudioSegement(string audioFile)
        {
            var preferences = new Preferences("en-US", new Uri("wss://speech.platform.bing.com/api/service/recognition/continuous"), new CognitiveServicesAuthorizationProvider("68ecbfed77384b0badae81995a5b256b"));
            using (var speechClient = new SpeechClient(preferences))
            {
                speechClient.SubscribeToPartialResult(this.OnPartialResult);
                speechClient.SubscribeToRecognitionResult(this.OnRecognitionResult);
                using (var audio = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                {
                    var deviceMetadata = new DeviceMetadata(DeviceType.Near, DeviceFamily.Desktop, NetworkType.Ethernet, OsName.Windows, "1607", "Dell", "T3600");
                    var applicationMetadata = new ApplicationMetadata("TranscriptionApp", "1.0.0");
                    var requestMetadata = new RequestMetadata(Guid.NewGuid(), deviceMetadata, applicationMetadata, "TranscriptionService");
                    await speechClient.RecognizeAsync(new SpeechInput(audio, requestMetadata), this.cts.Token).ConfigureAwait(false);
                }
            }
        }
        public Task OnPartialResult(RecognitionPartialResult args)
        {
            return CompletedTask;
        }
        public async Task OnRecognitionResult(RecognitionResult args)
        {
            if (args.Phrases.Count > 0)
            {
                int fileIndex = await this.StateManager.GetStateAsync<int>("FileIndex");
                string bestText = args.Phrases[0].DisplayText;
                if (mTexts.ContainsKey(fileIndex))
                    mTexts[fileIndex] += bestText;
                else
                    mTexts.Add(fileIndex, bestText);
                if (bestText.ToLower().IndexOf("gonna kill you") >= 0)
                {
                    await this.StateManager.SetStateAsync<bool>("ActiveFile", false);
                    await this.StateManager.SaveStateAsync();
                }
            }
        }
        private async Task uploadTranscript()
        {
            string fileName = await this.StateManager.GetStateAsync<string>("FileName");

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("transcripts");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName + "transcript.txt");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mTexts.Count; i++)
            {
                sb.Append(mTexts[i]);
                //for (int j = 0; j < result.Count; j++)
                //    sb.Append(result[0].DisplayText);
            }
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(sb.ToString()), false))
            {
                await blockBlob.UploadFromStreamAsync(stream);
            }
        }
        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            int fileCount = state[0];

            if (reminderName == "Transcription")
            {
                bool isActive = await this.StateManager.GetStateAsync<bool>("ActiveFile");
                if (!isActive)
                {
                    List<string> files = await this.StateManager.GetStateAsync<List<string>>("Files");
                    if (files.Count == 0)
                    {
                        if (mTexts.Count == fileCount)
                        {
                            await this.UnregisterReminderAsync(mReminder);
                            await uploadTranscript();
                        }
                        else
                            return;
                    }
                    int fileIndex = await this.StateManager.GetStateAsync<int>("FileIndex") + 1;
                    string fileName = files[0];
                    files.RemoveAt(0);
                    await this.StateManager.SetStateAsync<List<string>>("Files", files);
                    await this.StateManager.SetStateAsync<int>("FileIndex", fileIndex);
                    await this.StateManager.SetStateAsync<bool>("ActiveFile", true);
                    await this.StateManager.SaveStateAsync();
                    await transcribeAudioSegement(fileName);
                }
            }
        }
    }
}
