using ArchiBot.CrawlerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ArchiBot.ArchiGraph;
using ArchiBot.GraphDBClient;
using System.Configuration;

namespace ArchiBot.CrawlerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TypeTree nodeTypeTree = TypeTree.LoadFromJson("arm-node-type-tree.json");
#if !DEBUG
            if (switchIndex(args, "v")>=0)
#endif
            {
                Trace.Listeners.Clear();
                Trace.Listeners.Add(new ConsoleTraceListener());
            }

            var token = readParameter(args, "t");
#if !DEBUG
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("GitHub authentication token is required (pass it in using -t switch).");
                return;
            }
#else
            token = ConfigurationManager.AppSettings["GitHubToken"];
#endif

            var crawlerManagers = new CrawlerManager[]
            {
                new AzureQuickstartCrawlerManager(token)
            };

            var loop = Parallel.ForEach<CrawlerManager>(crawlerManagers,
                (manager) =>
                {
                    var graphs = manager.CrawlAsync().Result;
                    var graphDB = new GraphDBClient.CosmosDBClient(ConfigurationManager.AppSettings["CosmosDBEndpoint"],
                        ConfigurationManager.AppSettings["CosmosDBAuthKey"]);
                    graphDB.Connect().WriteGraphs(ConfigurationManager.AppSettings["GraphDatabase"],
                        ConfigurationManager.AppSettings["GraphCollection"], graphs);
                });

            while (!loop.IsCompleted)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Done!");
        }
        private static int switchIndex(string[] args, string cmdSwitch)
        {
            if (args == null)
                return -1;
            int index = Array.IndexOf(args, "-" + cmdSwitch);
            if (index >= 0)
                return index;
            index = Array.IndexOf(args, "--" + cmdSwitch);
            if (index >= 0)
                return index;
            return -1;
        }
        private static string readParameter(string[] args, string cmdSwitch)
        {
            int index = switchIndex(args, "t");
            if (index < 0 || index >= args.Length -1)
                return "";
            return args[index + 1];
        }
    }
}
