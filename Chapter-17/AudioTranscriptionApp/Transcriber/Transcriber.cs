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
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using StateAggregator.Interfaces;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Transcriber
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Transcriber : Actor, ITranscriber
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        StringBuilder mText = new StringBuilder(); 
        private readonly TimeSpan mReportInterval = TimeSpan.FromSeconds(2);
        private DateTime mLastReportTime = DateTime.Now;
        private TimeSpan mTotalTime;
        private string mFileName;
        private List<string> mFiles;

        public Transcriber(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public async Task SubmitJob(string url, bool isPublic, CancellationToken cancellationToken)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);


            string fileName = "";

            if (!isPublic)
            {
                fileName = await getAudioFileFromBlob(tempFolder, url);
                mFileName = url;
            }

            var audioFile = await convertToWav(fileName);

            mFiles = await splitFiles(audioFile.Item1);

            

            await reportProgress(url, 0, "Job created.");

            for (int i = 0; i < mFiles.Count; i++)
                await transcribeAudioSegement(mFiles[i]);
            await uploadTranscript();

        }

        private static async Task reportProgress(string url, int percent, string message)
        {
            Binding binding = WcfUtility.CreateTcpClientBinding();
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            var wcfClientFactory = new WcfCommunicationClientFactory<IStateAggregator>(
                clientBinding: binding,
                servicePartitionResolver: partitionResolver
                );
            var jobClient = new ServicePartitionClient<WcfCommunicationClient<IStateAggregator>>(
                wcfClientFactory,
                new Uri("fabric:/AudioTranscriptionApp/StateAggregator"));
            await jobClient.InvokeWithRetryAsync(client => client.Channel.ReportProgress(url, percent, message));
        }

        private async Task<Tuple<string, TimeSpan>> convertToWav(string file)
        {
            string fileWithouExtension = Path.Combine(Path.GetFullPath(file).Replace(Path.GetFileName(file), ""), Path.GetFileNameWithoutExtension(file));
            string outFile = file;            
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
                mTotalTime = maleReader.TotalTime;
            }
           
            return await Task.FromResult<Tuple<string,TimeSpan>>(new Tuple<string, TimeSpan>(outFile, mTotalTime));
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
            //var preferences = new Preferences("en-US", new Uri("wss://9598b5e5964e4e9b881fd85f5d380713.api.cris.ai/ws/cris/speech/recognize/continuous"), new CognitiveServicesAuthorizationProvider("36677b4f10da4d2a946af66da757ef0b"));
            //var preferences = new Preferences("en-US", new Uri("wss://06a2285998274ed6a57103a41df672b3.api.cris.ai/ws/cris/speech/recognize/continuous"), new CognitiveServicesAuthorizationProvider("36677b4f10da4d2a946af66da757ef0b"));
            //var preferences = new Preferences("en-US", new Uri("wss://5ba5d066af03405ba71e84ba3bc4d185.api.cris.ai/ws/cris/speech/recognize/continuous"), new CognitiveServicesAuthorizationProvider("36677b4f10da4d2a946af66da757ef0b"));
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
        public async Task OnPartialResult(RecognitionPartialResult args)
        {
            if (DateTime.Now - mLastReportTime >= mReportInterval)
            {
                var percent = (int)(args.MediaTime * 0.00001 / mTotalTime.TotalSeconds);
                await reportProgress(mFileName, percent, args.DisplayText.Substring(0, Math.Min(args.DisplayText.Length, 50)) + "...");
                mLastReportTime = DateTime.Now;
            }
        }
        public async Task OnRecognitionResult(RecognitionResult args)
        {
            if (args.Phrases.Count > 0)
            {
                string bestText = args.Phrases[args.Phrases.Count-1].DisplayText;
                mText.Append(bestText);
            }
            await Task.FromResult<bool>(true);
        }
        private async Task uploadTranscript()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("transcripts");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(mFileName + "transcript.txt");
            
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(mText.ToString()), false))
            {
                await blockBlob.UploadFromStreamAsync(stream);
            }

            Binding binding = WcfUtility.CreateTcpClientBinding();
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            var wcfClientFactory = new WcfCommunicationClientFactory<IStateAggregator>(
                clientBinding: binding,
                servicePartitionResolver: partitionResolver
                );
            var jobClient = new ServicePartitionClient<WcfCommunicationClient<IStateAggregator>>(
                wcfClientFactory,
                new Uri("fabric:/AudioTranscriptionApp/StateAggregator"));
            await jobClient.InvokeWithRetryAsync(client => client.Channel.ReportCompletion(mFileName, blockBlob.Uri.AbsoluteUri));
        }

    }
}
