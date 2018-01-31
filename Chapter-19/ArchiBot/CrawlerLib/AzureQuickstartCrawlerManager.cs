using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.CrawlerLib
{
    public class AzureQuickstartCrawlerManager:CrawlerManager
    {
        private string mToken;
        public AzureQuickstartCrawlerManager(string token):
            base(new string[] { "https://github.com/Azure/azure-quickstart-templates"})
        {
            mToken = token;
        }
        public override Task<List<CrawlJob>> CreateJobs()
        {
            return base.CreateJobsFromGitHubFolders("Azure", "azure-quickstart-templates", mToken,
                new [] { 
                    "1-CONTRIBUTION-GUIDE",
                    "100-blank-template",
                    "test"
                },  "azuredeploy.json");
        }
        protected override Crawler CreateCrawler()
        {
            return new AzureQuickstartCrawler();
        }
    }
}
