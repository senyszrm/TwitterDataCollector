using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Timers;
using SampledStreamServer.Models;
using SampledStreamServer.Database;
using Microsoft.EntityFrameworkCore;

namespace SampledStreamServer.Services
{
    public class TwitterSampledStreamProcessor : BackgroundService, ITwitterEndpointProcessor
    {
        // Keeps track of the data that needs to be processed still (obtained from the endpoint capturer)
        private readonly BlockingCollection<string> dataToProcess;

        // Indicates if the processing is in progress
        private bool processingInProgress = false;

        // Reference to which hashtag parser we will use to process hashtags (interface, implementation is provided via constructor)
        private readonly IHashtagParser hashtagParser;

        // Stores Hashtag information that is currently being collected this run. Key = Hashtag
        private readonly HashSet<string> hashtags = new();

        // The database that will be used to store the processed data
        private readonly SampledStreamDbContext sampledStreamDb;

        // Returns the Top Ten Hashtags encountered during the collection period
        public List<KeyValuePair<string, uint>> topTenHashtags
        {
            get
            {
                return sampledStreamDb.SummaryData?.GetTopTenHashtags(sampledStreamDb) ?? new List<KeyValuePair<string, uint>>();
            }
        }

        // Stores the total count of tweets processed during the collection period
        public uint totalTweets
        {
            get
            {
                return sampledStreamDb.SummaryData.TotalTweets;
            }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="capturer">The capturer that contains captured data to be processed by this endpoint processor</param>
        ///<param name="parser">The Hashtag parser that will be used to parse hashtags encountered in the captured data</param>
        ///<param name="sampledStreamDb">The database context which will be used to store data that is being processed into the database</param>
        public TwitterSampledStreamProcessor(ITwitterEndpointCapture capturer, IHashtagParser parser, SampledStreamDbContext sampledStreamDb)
        {
            this.dataToProcess = capturer.capturedData;
            this.hashtagParser = parser;
            this.sampledStreamDb = sampledStreamDb;
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="dataToProcess">The captured data that will be processed by this endpoint processor</param>
        ///<param name="parser">The Hashtag parser that will be used to parse hashtags encountered in the captured data</param>
        ///<param name="sampledStreamDb">The database context which will be used to store data that is being processed into the database</param>
        public TwitterSampledStreamProcessor(BlockingCollection<string> dataToProcess, IHashtagParser parser, SampledStreamDbContext sampledStreamDb)
        {
            this.dataToProcess = dataToProcess;
            this.hashtagParser = parser;
            this.sampledStreamDb = sampledStreamDb;
        }

        public override async Task StartAsync(CancellationToken token)
        {
            await ExecuteAsync(token);
        }

        ///<summary>
        /// Given a list of captured data provided via constructor, will process the data in an appropriate fashion per endpoint type
        ///</summary>
        ///<returns>A Task that can be awaited to wait for data to process</returns>
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                // This is the main loop which processes the statistics for the Tweets encountered
                processingInProgress = true;
                SummaryData summaryData = this.sampledStreamDb.SummaryData;

                foreach (var hashtag in this.sampledStreamDb.Hashtags)
                {
                    // If we haven't encountered this hashtag yet, add it to the Dictionary with appropriate count
                    if (!hashtags.Contains(hashtag.Name))
                    {
                        hashtags.Add(hashtag.Name);
                    }
                }
                while (!token.IsCancellationRequested && processingInProgress)
                {
                    string? tweetJsonStr = "";
                    
                    try
                    {
                        tweetJsonStr = dataToProcess.Take(token);
                    } catch(OperationCanceledException)
                    {
                        // Do nothing as this is a graceful cancellation request from the user
                    }

                    // Remove a tweet from the queue so that we can process it
                    if (tweetJsonStr != "")
                    {
                        // Increment the total count of Tweets processed so far
                        summaryData.TotalTweets++;

                        // Convert the JSON object Tweet into a C# object so that it can be processed
                        Tweet? tweet = JsonSerializer.Deserialize<Tweet>(tweetJsonStr);

                        // Match the text portion of the tweet against the Hashtag Regex to find each occurrence of the Hashtag
                        var foundHashtags = hashtagParser.Parse(tweet?.data?.text ?? "");
                        foreach (string foundHashtag in foundHashtags)
                        {
                            // If we haven't encountered this hashtag yet, add it to the Dictionary with 0 count
                            if (!hashtags.Contains(foundHashtag))
                            {
                                hashtags.Add(foundHashtag);
                            }

                            // Check if we found this hashtag yet in the database
                            var foundHashtagsDb = sampledStreamDb.Hashtags.Where(h => h.Name == foundHashtag).Include(h => h.ObservedTimes).ToList();
                            Hashtag? foundHashtagDb;
                            if (foundHashtagsDb.Count == 0)
                            {
                                foundHashtagDb = new Hashtag { Name = foundHashtag };
                                foundHashtagDb.ObservedTimes = new List<HashtagOccurrence> { new HashtagOccurrence { TimeOfOccurrence = DateTime.Now } };
                                sampledStreamDb.Hashtags.Add(foundHashtagDb);

                                // TODO: Look into doing this in bulk to improve performance
                                sampledStreamDb.SaveChanges();
                            }
                            else
                            {
                                foundHashtagDb = foundHashtagsDb.SingleOrDefault();
                                foundHashtagDb?.ObservedTimes.Add(new HashtagOccurrence { TimeOfOccurrence = DateTime.Now });
                            }
                        }
                    }
                }
            });
        }

        ///<summary>
        /// Stops a currently in progress data processor
        ///</summary>
        public override async Task StopAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                processingInProgress = false;
            });
        }
    }
}
