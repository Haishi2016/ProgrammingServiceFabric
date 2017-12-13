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
using System.Collections.Concurrent;

namespace Transcriber
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Transcriber : Actor, ITranscriber
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly TimeSpan mReportInterval = TimeSpan.FromMilliseconds(500);
        private CancellationTokenSource mTokensource;
        private CancellationToken mToken;
        public Transcriber(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            mTokensource = new CancellationTokenSource();
            mToken = mTokensource.Token;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public async Task SubmitJob(string url)
        {
            await reportProgress(url, 0, "Job created.");

            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            TimeSpan totalTime = TimeSpan.Zero;
            await Task.Factory.StartNew(() =>
            {
                getAudioFileFromBlob(tempFolder, url, mToken)
                .ContinueWith<Tuple<string, TimeSpan>>((fileName) =>
                {
                    return convertToWav(fileName.Result, mToken).Result;
                }, mToken)
                .ContinueWith<List<string>>((audioFile) =>
                {
                    totalTime = audioFile.Result.Item2;
                    return splitFiles(audioFile.Result.Item1).Result;
                }, mToken)
                .ContinueWith((files) =>
                {
                    var tasks = new ConcurrentBag<Task<Tuple<string, string>>>();
                    var fileResults = files.Result;
                    for (int i = 0; i < fileResults.Count(); i++)
                    {
                        tasks.Add(transcribeAudioSegement(url, fileResults[0], totalTime, mToken));
                    }
                    return Task.WhenAll<Tuple<string, string>>(tasks).Result;
                }, mToken)
                .ContinueWith((results) =>
                {
                    var detectionResult = results.Result[0];
                    var fileName = url; // detectionResult.Item1;
                    var text = detectionResult.Item2;
                    return uploadTranscript(fileName, text);
                }, mToken);
            });
        }
        public async Task DeleteJob(string name)
        {
            mTokensource.Cancel();
            await reportCancellation(name);
            await deleteFiles(name);

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
        private static async Task reportCancellation(string url)
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
            await jobClient.InvokeWithRetryAsync(client => client.Channel.ReportCancellation(url));
        }
        private async Task<Tuple<string, TimeSpan>> convertToWav(string file, CancellationToken token)
        {
            string fileWithouExtension = Path.Combine(Path.GetFullPath(file).Replace(Path.GetFileName(file), ""), Path.GetFileNameWithoutExtension(file));
            string outFile = file;            
            if (file.ToLower().EndsWith(".mp3"))
            {
                outFile = fileWithouExtension + ".wav";
                token.ThrowIfCancellationRequested();
                using (Mp3FileReader reader = new Mp3FileReader(file))
                {
                    WaveFileWriter.CreateWaveFile(outFile, reader);
                }
            }
            token.ThrowIfCancellationRequested();
            TimeSpan totalTime;
            using (WaveFileReader maleReader = new WaveFileReader(outFile))
            {
                totalTime = maleReader.TotalTime;
            }
           
            return await Task.FromResult<Tuple<string,TimeSpan>>(new Tuple<string, TimeSpan>(outFile, totalTime));
        }
        private async Task<List<string>> splitFiles(string file)
        {
            return await Task.FromResult<List<string>>(new List<string> { file });
        }
        private async Task<string> getAudioFileFromBlob(string tempFolder, string url, CancellationToken token)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("clips");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(url);
            string fileName = Path.Combine(tempFolder, url);
            token.ThrowIfCancellationRequested();
            await blockBlob.DownloadToFileAsync(fileName, FileMode.Create);
            return fileName;
        }
        private async Task deleteFiles(string url)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("clips");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(url);
            if (blockBlob.Exists())
                await blockBlob.DeleteAsync();
            container = blobClient.GetContainerReference("transcripts");
            blockBlob = container.GetBlockBlobReference(url + "transcript.txt");
            if (blockBlob.Exists())
                await blockBlob.DeleteAsync();
        }
        private Task<Tuple<string,string>> transcribeAudioSegement(string fileName, string audioFile, TimeSpan totalTime, CancellationToken token)
        {
            return Task.Factory.StartNew<Tuple<string,string>>(() =>
            {
                var preferences = new Preferences("en-US", new Uri("wss://speech.platform.bing.com/api/service/recognition/continuous"), new CognitiveServicesAuthorizationProvider("68ecbfed77384b0badae81995a5b256b"));
                DateTime lastReportTime = DateTime.Now;
                StringBuilder text = new StringBuilder();
                using (var speechClient = new SpeechClient(preferences))
                {
                    speechClient.SubscribeToPartialResult((args) =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            token.ThrowIfCancellationRequested();
                            if (DateTime.Now - lastReportTime >= mReportInterval)
                            {
                                var percent = (int)(args.MediaTime * 0.00001 / totalTime.TotalSeconds);
                                reportProgress(fileName, percent, args.DisplayText.Substring(0, Math.Min(args.DisplayText.Length, 50)) + "...").Wait();
                                lastReportTime = DateTime.Now;
                            }
                        });
                    });
                    speechClient.SubscribeToRecognitionResult((args) =>
                    {
                        return Task.Factory.StartNew(() =>
                            {
                                if (args.Phrases.Count > 0)
                                {
                                    string bestText = args.Phrases[args.Phrases.Count - 1].DisplayText;
                                    text.Append(bestText);
                                }
                            });
                    });
                    using (var audio = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                    {
                        var deviceMetadata = new DeviceMetadata(DeviceType.Near, DeviceFamily.Desktop, NetworkType.Ethernet, OsName.Windows, "1607", "Dell", "T3600");
                        var applicationMetadata = new ApplicationMetadata("TranscriptionApp", "1.0.0");
                        var requestMetadata = new RequestMetadata(Guid.NewGuid(), deviceMetadata, applicationMetadata, "TranscriptionService");
                        speechClient.RecognizeAsync(new SpeechInput(audio, requestMetadata), this.cts.Token).Wait();
                        return new Tuple<string, string>(audioFile, text.ToString());
                    }
                }
            });
        }
        
        private async Task uploadTranscript(string fileName, string text)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("transcripts");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName + "transcript.txt");
            
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(text), false))
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
            await jobClient.InvokeWithRetryAsync(client => client.Channel.ReportCompletion(fileName, blockBlob.Uri.AbsoluteUri));
        }

    }
}
