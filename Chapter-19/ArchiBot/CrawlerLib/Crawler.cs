
using ArchiBot.ArchiGraph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.CrawlerLib
{
    public abstract class Crawler
    {
        public abstract Task<List<AppGraph>> Handle(CrawlJob job);
    }
}
