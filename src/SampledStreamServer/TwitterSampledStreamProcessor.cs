using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Timers;

namespace SampledStreamServer
{
    public class TwitterSampledStreamProcessor : TwitterEndpointProcessor
    {
        // Keeps track of the data that needs to be processed still (obtained from the endpoint capturer)
        private ConcurrentQueue<string> dataToProcess;

        // Indicates if the processing is in progress
        private bool processingInProgress = false;

        // Reference to which hashtag parser we will use to process hashtags (interface, implementation is provided via constructor)
        private HashtagParser hashtagParser;

        // Stores Hashtag information that is currently being collected this run. Key = Hashtag, Value = Count of times that Hashtag has been encountered
        private Dictionary<string, uint> hashtags = new Dictionary<string, uint>();

        // Returns the Top Ten Hashtags encountered during the collection period
        public List<KeyValuePair<string, uint>> topTenHashtags
        {
            get
            {
                return hashtags.OrderByDescending(d => d.Value).Take(10).ToList();
            }
        }

        // Stores the total count of tweets processed during the collection period
        public uint totalTweets { get; private set; }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="dataToProcess">The captured data that will be processed by this endpoint processor</param>
        ///<param name="parser">The Hashtag parser that will be used to parse hashtags encountered in the captured data</param>
        public TwitterSampledStreamProcessor(ConcurrentQueue<string> dataToProcess, HashtagParser parser)
        {
            this.dataToProcess = dataToProcess;
            this.hashtagParser = parser;
        }

        ///<summary>
        /// Given a list of captured data provided via constructor, will process the data in an appropriate fashion per endpoint type
        ///</summary>
        ///<returns>A Task that can be awaited to wait for data to process</returns>
        public async Task ProcessCapturedData()
        {
            await Task.Run(() =>
            {
                // This is the main loop which processes the statistics for the Tweets encountered
                processingInProgress = true;
                while (processingInProgress)
                {
                    string? tweetJsonStr = "";

                    // Remove a tweet from the queue so that we can process it
                    if (dataToProcess.Count > 0 && dataToProcess.TryDequeue(out tweetJsonStr) && tweetJsonStr != "")
                    {
                        // Increment the total count of Tweets processed so far
                        totalTweets++;

                        // Convert the JSON object Tweet into a C# object so that it can be processed
                        TwitterDataCollector.Tweet? tweet = JsonSerializer.Deserialize<TwitterDataCollector.Tweet>(tweetJsonStr);

                        // Match the text portion of the tweet against the Hashtag Regex to find each occurrence of the Hashtag
                        var foundHashtags = hashtagParser.Parse(tweet?.data?.text ?? "");
                        foreach (string foundHashtag in foundHashtags)
                        {
                            // If we haven't encountered this hashtag yet, add it to the Dictionary with 0 count
                            if (!hashtags.ContainsKey(foundHashtag))
                            {
                                hashtags.Add(foundHashtag, 0);
                            }

                            // Increment the amount of times we've seen this hashtag
                            hashtags[foundHashtag]++;
                        }
                    }
                }
            });
        }

        ///<summary>
        /// Stops a currently in progress data processor
        ///</summary>
        public void StopProcessingData()
        {
            processingInProgress = false;
        }
    }
}
