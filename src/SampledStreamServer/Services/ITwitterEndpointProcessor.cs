using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampledStreamServer.Services
{
    public interface ITwitterEndpointProcessor : IHostedService
    {
        // Returns the Top Ten Hashtags encountered during the collection period
        public List<KeyValuePair<string, uint>> topTenHashtags { get; }

        // Stores the total count of tweets processed during the collection period
        public uint totalTweets { get; }
    }
}
