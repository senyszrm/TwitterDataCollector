using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterDataCollector
{
    // These classes are used to marshall the JSON data collected from the Twitter API v2 Sampled Stream Endpoint into C# objects
    internal class Tweet
    {
        public TweetData? data { get; set; }
    }
    internal class TweetData
    {
        public string? id { get; set; }
        public string? text { get; set; }

    }
}
