using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.ServiceFabric.Actors.Client;
using Transcriber.Interfaces;
using Microsoft.ServiceFabric.Actors;
using StateAggregator.Interfaces;
using System.ServiceModel.Channels;

namespace WebFrontend.Controllers
{
    [Route("api/[controller]")]
    public class JobController : Controller
    {
        private static Random mRand = new Random();

        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public async Task<IEnumerable<JobStatus>> Jobs()
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
            var result = await jobClient.InvokeWithRetryAsync(client => client.Channel.ListJobs());
            return result;
            //var rng = new Random();
            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
            //    TemperatureC = rng.Next(-20, 55),
            //    Summary = Summaries[rng.Next(Summaries.Length)]
            //});
        }
        [HttpPost("[action]"), DisableRequestSizeLimit]
        public async Task<IActionResult> SubmitFileJob()
        {
            //var configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            //string connectionString = configPackage.Settings.Sections["JobManagerConfig"].Parameters["StorageConnectionString"].Value;
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=transcriptions;AccountKey=5Aoic2oWz+n8jUP4tonlzoic9/rapEJPG0jDHHccTLEp2KfnkHusFTS9TWF6mzDBKPixlVQUZ4/QYlQu3wLSpw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("clips");
            foreach (var file in Request.Form.Files)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.Name);
                await blockBlob.UploadFromStreamAsync(file.OpenReadStream());
                ITranscriber proxy = ActorProxy.Create<ITranscriber>(new ActorId(Guid.NewGuid()), new Uri("fabric:/AudioTranscriptionApp/TranscriberActorService"));
                try
                {
                    await proxy.SubmitJob(file.Name, false, new System.Threading.CancellationToken());
                }
                catch (Exception exp)
                {
                    string a = exp.Message;
                }
            }        
            return Ok();
        }

        public class WeatherForecast
        {
            public string Name { get; set; }
            public int Percentage { get; set; }
            public string Message { get; set; }
            public string Url { get; set; }            
        }
    }
}
