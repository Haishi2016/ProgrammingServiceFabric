using ArchiBot.ArchiGraph;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiBot.CrawlerLib
{
    public abstract class  CrawlerManager
    {
        private string[] mSources;
        public CrawlerManager(string[] sources)
        {
            if (sources == null || sources.Length == 0)
                throw new ArgumentNullException("CrawlerManager has to be initialized with a list of data source addresses.");

            mSources = (string[])sources.Clone();
        }
        protected async Task<List<CrawlJob>> CreateJobsFromGitHubFolders(string gitHubOwner, string gitHubRepo, string token, string[] ignoreList, string templateNamePattern)
        {
            List<CrawlJob> jobs = new List<CrawlJob>();
            var gitHub = new GitHubClient(new ProductHeaderValue("archi-bot"));
            var tokenAuth = new Credentials(token);
            gitHub.Credentials = tokenAuth;
            var contents = await gitHub.Repository.Content.GetAllContents(gitHubOwner, gitHubRepo);
            var folders = from f in contents
                          where f.Type == ContentType.Dir
                          && (ignoreList == null || Array.IndexOf(ignoreList, f.Path) < 0)
                          select f;
            foreach(var folder in folders)
            {
                var folderContents = gitHub.Repository.Content.GetAllContents(gitHubOwner, gitHubRepo, folder.Path).Result;
                var templateFile = (from f in folderContents
                                    where f.Name == templateNamePattern
                                    select f).FirstOrDefault();
                if (templateFile != null)
                {
                    var job = new CrawlJob(templateFile.Path, templateFile.DownloadUrl);
                    Trace.TraceInformation("Job created - Name:{0}, Download Uri:{1}", job.Name, job.DownloadUrl);
                    jobs.Add(job);
#if DEBUG
                    break;
#endif
                }
            }
            return jobs;
        }
        public async Task<List<AppGraph>> CrawlAsync()
        {
            List<AppGraph> ret = new List<AppGraph>();

            await CreateJobs().ContinueWith(
                (jobs) =>
                {                    
                    int parallelism = 5;
#if DEBUG
                    parallelism = 1;
#endif
                    int chunkSize = jobs.Result.Count / parallelism;
                    if (jobs.Result.Count % parallelism != 0)
                        parallelism++;
                    Parallel.For(0, parallelism, (chunk) =>
                     {
                         Crawler crawler = CreateCrawler();
                         for (int i = 0; i < chunkSize; i++)
                         {
                             if (chunkSize * chunk + i > jobs.Result.Count - 1)
                                 break;
                             var graphs = crawler.Handle(jobs.Result[chunkSize * chunk + i]).Result;
                             if (graphs != null)
                                ret.AddRange(crawler.Handle(jobs.Result[chunkSize * chunk + i]).Result);
#if DEBUG
                             break;
#endif
                         }
                     });
                });

            return ret;
        }
        public virtual async Task<List<CrawlJob>> CreateJobs ()
        {
            return await Task.FromException<List<CrawlJob>>(new InvalidOperationException());
        }
        protected virtual Crawler CreateCrawler()
        {
            throw new InvalidOperationException();
        }
    }
}
