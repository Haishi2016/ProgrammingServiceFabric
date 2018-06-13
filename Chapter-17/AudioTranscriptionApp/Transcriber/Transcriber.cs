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
using System.Text.RegularExpressions;

namespace Transcriber
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Transcriber : Actor, ITranscriber
    {        
        private readonly TimeSpan mReportInterval = TimeSpan.FromMilliseconds(500);
        private CancellationTokenSource mTokensource;
        private CancellationToken mToken;
        private int[] percentages;
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

        public async Task SubmitJob(string url, string user)
        {
            await reportProgress(url, 0, "Job created.", user);

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
                    percentages = new int[fileResults.Count()];
                    for (int i = 0; i < fileResults.Count(); i++)
                    {
                        tasks.Add(transcribeAudioSegement(i, url, fileResults[i], totalTime, mToken, user));
                    }
                    return Task.WhenAll<Tuple<string, string>>(tasks).Result;
                }, mToken)
                .ContinueWith((results) =>
                {
                    var sorted = (from a in results.Result
                                  orderby a.Item1
                                  select a).ToList();
                    string text = "";
                    for (int i = 0; i < sorted.Count; i++)
                        text = mergeStrings(text, sorted[i].Item2, 50);                    
                    var fileName = url; // detectionResult.Item1;
                    return uploadTranscript(fileName, text, user);
                }, mToken);
            });
        }

        private double stringDistance(string a, string b)
        {
            string wa = a.ToUpper().Trim();
            string wb = b.ToUpper().Trim();
            if (wa.Length == 0 && wb.Length == 0)
                return 0;
            double distance = 0;
            for (int i = 0; i < Math.Min(wa.Length, wb.Length); i++)
                distance += Math.Abs(wa[i] - wb[i]);
            for (int i = Math.Min(wa.Length, wb.Length); i < Math.Max(wa.Length, wb.Length); i++)
                distance += 26;
            return distance / (Math.Max(wa.Length, wb.Length) * 26);
        }
        private string mergeStrings(string a, string b, int windowSize)
        {
            string[] arrayA = a.Replace('.', ' ').Replace(',', ' ').Split(' ');
            string[] arrayB = b.Replace('.', ' ').Replace(',', ' ').Split(' ');
            int matchSize = Math.Min(arrayB.Length, Math.Min(arrayA.Length, windowSize));
            List<string> listB = arrayB.Take(matchSize).ToList();
            int aIndex = arrayA.Length - matchSize;
            int[] bestDistance = new int[matchSize];
            for (int permu = 0; permu < matchSize; permu++)
            {
                int matchCount = 0;
                for (int i = 0; i < matchSize; i++)
                {
                    if (stringDistance(arrayA[aIndex + i], listB[i]) < 0.1)
                        matchCount++;
                }
                string first = listB[0];
                listB.RemoveAt(0);
                listB.Add(first);
                bestDistance[permu] = matchCount;
            }

            int bestIndex = -1;
            int bestCount = 0;
            for (int i = 0; i < matchSize; i++)
            {
                if (bestDistance[i] > bestCount)
                {
                    bestCount = bestDistance[i];
                    bestIndex = i;
                }
            }
            if (bestIndex < 0)
                return (a + " " + b).Replace("  ", " ").Trim();
            else
            {
                listB = arrayB.Take(matchSize).ToList();

                for (int i = 0; i < bestIndex; i++)
                {
                    string first = listB[0];
                    listB.RemoveAt(0);
                    listB.Add(first);
                }
                int lastIndex = -1;
                List<Tuple<int, int>> longestMatches = new List<Tuple<int, int>>();
                bool inList = false;
                for (int i = 0; i < matchSize; i++)
                {
                    if (stringDistance(arrayA[aIndex + i], listB[i]) <= 0.1)
                    {
                        if (!inList)
                        {
                            inList = true;
                            longestMatches.Add(new Tuple<int, int>(i, -1));
                        }
                    }
                    else
                    {
                        if (inList)
                        {
                            inList = false;
                            longestMatches[longestMatches.Count - 1] = new Tuple<int, int>(longestMatches[longestMatches.Count - 1].Item1, i);
                        }
                    }
                }
                int maxLength = 0;
                for (int i = 0; i < longestMatches.Count; i++)
                {
                    if (longestMatches[i].Item2 - longestMatches[i].Item1 > maxLength)
                    {
                        lastIndex = longestMatches[i].Item1;
                        maxLength = longestMatches[i].Item2 - longestMatches[i].Item1;
                    }
                }
                if (lastIndex < 0)
                    return (a + " " + b).Replace("  ", " ").Trim();
                else
                {
                    int lengthA = 0, lengthB = 0;
                    for (int i = lastIndex; i < matchSize; i++)
                        lengthA += arrayA[aIndex + i].Length + 1;
                    if (lengthA > 0)
                        lengthA--;
                    for (int i = 0; i < (lastIndex + bestIndex) % matchSize; i++)
                        lengthB += arrayB[i].Length + 1;
                    if (lengthB > 0)
                        lengthB--;
                    return (a.Substring(0, a.Length - lengthA) + " " + b.Substring(lengthB)).Replace("  ", " ").Trim();
                }
            }
        }

        public async Task DeleteJob(string name)
        {
            mTokensource.Cancel();
            await reportCancellation(name);
            await deleteFiles(name);
            mTokensource.Dispose();
            mTokensource = new CancellationTokenSource();
            mToken = mTokensource.Token;
        }
        private static async Task reportProgress(string url, int percent, string message, string user)
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
            await jobClient.InvokeWithRetryAsync(client => client.Channel.ReportProgress(url, percent, message, user));
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
            } else if (file.ToLower().EndsWith(".m4a"))
            {
                outFile = fileWithouExtension + ".wav";
                token.ThrowIfCancellationRequested();
                using (MediaFoundationReader reader = new MediaFoundationReader(file))
                {
                    using (ResamplerDmoStream resampledReader = new ResamplerDmoStream(reader,
                        new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels)))
                    using (WaveFileWriter waveWriter = new WaveFileWriter(outFile, resampledReader.WaveFormat))
                    {
                        resampledReader.CopyTo(waveWriter);
                    }
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
        private  Task<List<string>> splitFiles(string file)
        {
            List<string> outputFiles = new List<string>();
            int chunkSizeinSeconds = 6000;
            int overlapInSeconds = 5;
            string outputFilePattern = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + "{0}.wav";
            using (WaveFileReader reader = new WaveFileReader(file))
            {
                int bufferSize = reader.WaveFormat.AverageBytesPerSecond;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                int fileCount = 1;
                string fileName = string.Format(outputFilePattern, fileCount);
                WaveFileWriter writer = null;
                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (writer == null)
                        writer = new WaveFileWriter(string.Format(outputFilePattern, fileCount), reader.WaveFormat);
                    writer.Write(buffer, 0, bytesRead);
                    if (reader.Position >= reader.Length - 1 || reader.Position >= chunkSizeinSeconds * bufferSize * fileCount)
                    {
                        outputFiles.Add(string.Format(outputFilePattern, fileCount));
                        writer.Close();
                        writer.Dispose();
                        writer = null;
                        fileCount++;
                        if (reader.Position < reader.Length)
                            reader.Position -= bufferSize * overlapInSeconds;
                    }
                }
            }
            return Task.FromResult<List<string>>(outputFiles);
        }
        private async Task<string> getAudioFileFromBlob(string tempFolder, string url, CancellationToken token)
        {
            string connectionString = "[Storage Account Connection String]";
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
            string connectionString = "[Storage Account Connection String]";
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
        private Task<Tuple<string,string>> transcribeAudioSegement(int index, string fileName, string audioFile, TimeSpan totalTime, CancellationToken token, string user)
        {
            return Task.Factory.StartNew<Tuple<string,string>>(() =>
            {
                //var preferences = new Preferences("en-US", new Uri("wss://speech.platform.bing.com/api/service/recognition/continuous"), new CognitiveServicesAuthorizationProvider("68ecbfed77384b0badae81995a5b256b"));
                var preferences = new Preferences("en-US", new Uri("wss://5ba5d066af03405ba71e84ba3bc4d185.api.cris.ai/ws/cris/speech/recognize/continuous"), new CognitiveServicesAuthorizationProvider("36677b4f10da4d2a946af66da757ef0b"));
                DateTime lastReportTime = DateTime.Now;
                DateTime lastDetectionTime = DateTime.Now;
                int runonLength = 0;
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
                                percentages[index] = percent;
                                reportProgress(fileName, Math.Min(99, percentages.Sum()), args.DisplayText.Substring(0, Math.Min(args.DisplayText.Length, 50)) + "...", user).Wait();
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
                                    runonLength += bestText.Length;
                                    if ((DateTime.Now - lastDetectionTime >= TimeSpan.FromSeconds(5) || runonLength >= 1800) && runonLength >= 250)
                                    {
                                        text.Append("\r\n\r\n    ");
                                        runonLength = 0;
                                    }
                                    text.Append(Regex.Replace(bestText, "(?<=[\\.,?])(?![$ ])", " "));
                                    lastDetectionTime = DateTime.Now;
                                }
                            });
                    });
                    using (var audio = new FileStream(audioFile, FileMode.Open, FileAccess.Read))
                    {
                        var deviceMetadata = new DeviceMetadata(DeviceType.Near, DeviceFamily.Desktop, NetworkType.Ethernet, OsName.Windows, "1607", "Dell", "T3600");
                        var applicationMetadata = new ApplicationMetadata("TranscriptionApp", "1.0.0");
                        var requestMetadata = new RequestMetadata(Guid.NewGuid(), deviceMetadata, applicationMetadata, "TranscriptionService");
                        speechClient.RecognizeAsync(new SpeechInput(audio, requestMetadata), mTokensource.Token).Wait();
                        return new Tuple<string, string>(audioFile, text.ToString());
                    }
                }
            });
        }
        
        private async Task uploadTranscript(string fileName, string text, string user)
        {
            string connectionString = "[Storage Account Connection String]";
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
            await jobClient.InvokeWithRetryAsync(client => client.Channel.ReportCompletion(fileName, blockBlob.Uri.AbsoluteUri, user));
        }

    }
}
