using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.CrawlerLib
{
    public class CrawlJob
    {
        public Uri DownloadUrl { get; set; }
        public string Name { get; set; }
        public CrawlJob(string name, Uri downloadUrl)
        {
            Name = name;
            DownloadUrl = downloadUrl;
        }
    }
}
