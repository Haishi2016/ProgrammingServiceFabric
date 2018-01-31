using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArchiBot.ArchiGraph;

namespace ArchiBot.CrawlerLib
{
    public class AzureQuickstartCrawler: Crawler
    {
        public override async Task<List<AppGraph>> Handle(CrawlJob job)
        {
            HttpClient client = new HttpClient();
            var jsonString = string.Empty;
            HttpResponseMessage response = client.GetAsync(job.DownloadUrl).Result;
            jsonString = await response.Content.ReadAsStringAsync();
            var template = JsonConvert.DeserializeObject<AzureARMObjectModels.Template>(jsonString);

            List<AppGraph> ret = new List<AppGraph>();
            foreach(var resource in template.Resources)
            {
                AppGraph graph = new AppGraph
                {
                };
                ret.Add(graph);
            }
            
            return ret;
        }
    }
}
