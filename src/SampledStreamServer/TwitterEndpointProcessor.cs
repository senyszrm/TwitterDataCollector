using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampledStreamServer
{
    public interface TwitterEndpointProcessor
    {
        // Returns the Top Ten Hashtags encountered during the collection period
        public List<KeyValuePair<string, uint>> topTenHashtags { get; }

        // Stores the total count of tweets processed during the collection period
        public uint totalTweets { get; }

        ///<summary>
        /// Given a list of captured data provided via constructor, will process the data in an appropriate fashion per endpoint type
        ///</summary>
        ///<returns>A Task that can be awaited to wait for data to process</returns>
        public Task ProcessCapturedData();

        ///<summary>
        /// Stops a currently in progress data processor
        ///</summary>
        public void StopProcessingData();
    }
}
