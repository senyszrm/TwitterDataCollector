using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Timers;

namespace TwitterDataCollector
{
    internal class TwitterSampleCollector
    {
        // Reference to the default location of the configuration XML file
        private const string DEFAULT_XML_PATH = "TwitterConnectionInfo.xml";

        // Expected XML Elements for the configuration file
        private const string EXPECTED_STREAM_ELEMENT = "sample_stream_url";
        private const string EXPECTED_BEARER_TOKEN_ELEMENT = "bearer_token";

        // This Regex is used to match hashtags. Processes alphanumeric Unicode characters so Hashtags from other languages will be present
        private const string HASHTAG_REGEX = @"(#+[\p{L}\p{N}(_)]{1,})";

        // Stores information needed to connect to the Twitter API v2 Sampled Stream Endpoint
        private string sampleStreamURL = "";
        private string userBearerToken = "";

        // Stores Hashtag information that is currently being collected this run. Key = Hashtag, Value = Count of times that Hashtag has been encountered
        private Dictionary<string, uint> hashtags = new Dictionary<string, uint>();

        // Queue of Tweets that we have yet to process. Processing involves deserializing the JSON tweet into C# objects then collecting metadata and storing them in the above dictionary
        private ConcurrentQueue<string> tweetsToProcess = new ConcurrentQueue<string>();

        // Stores the total count of tweets processed during the collection period
        public uint TotalTweets { get; private set; }

        // Returns the Top Ten Hashtags encountered during the collection period
        public List<KeyValuePair<string, uint>> TopTenHashtags {
            get {
                return hashtags.OrderByDescending(d => d.Value).Take(10).ToList();
            }
        }

        // Initialize the HTTP Client. We only expect a single client to be initialized in the entire program
        static readonly HttpClient client = new HttpClient();

        /// <summary>
        ///  Constructor which pulls required information from the provided configuration file
        /// </summary>
        /// <param name="configXmlPath">Path to the configuration XML file to be used</param>
        public TwitterSampleCollector(string configXmlPath = DEFAULT_XML_PATH)
        {
            XmlTextReader twitterConfigXmlReader = new XmlTextReader(configXmlPath);

            // Read the configuration XML file and look for the elements necessary to establish a Twitter connection
            while (twitterConfigXmlReader.Read())
            {
                twitterConfigXmlReader.MoveToElement();
                if (twitterConfigXmlReader.NodeType == XmlNodeType.Element && twitterConfigXmlReader.Name == EXPECTED_STREAM_ELEMENT)
                {
                    sampleStreamURL = twitterConfigXmlReader.ReadElementContentAsString().Trim();
                }
                else if (twitterConfigXmlReader.NodeType == XmlNodeType.Element && twitterConfigXmlReader.Name == EXPECTED_BEARER_TOKEN_ELEMENT)
                {
                    userBearerToken = twitterConfigXmlReader.ReadElementContentAsString().Trim();
                }

            }
            // If the URL or Bearer Token weren't set, throw an error
            if (sampleStreamURL == "")
            {
                throw new InvalidOperationException("Configuration file did not contain a valid Stream URL.");
            }
            if (userBearerToken == "")
            {
                throw new InvalidOperationException("Configuration file did not contain a valid Bearer Token.");
            }
        }

        /// <summary>
        ///  Establishes a connection to Twitter's API v2 Sampled Stream Endpoint and starts collecting data
        /// </summary>
        /// <param name="loggingInterval">Indicates how often to display collected data to the console (in milliseconds)</param>
        public async Task ConnectAndProcessTweets(uint loggingInterval)
        {
            try
            {
                // Set the Bearer token in the Authorization HTTP Headers
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userBearerToken);

                // Establish the connection to the Stream
                using (Stream tweetStream = await client.GetStreamAsync(sampleStreamURL))
                {
                    StreamReader tweetReader = new StreamReader(tweetStream);

                    // Starts listening on two threads. The first thread collects tweets but doesn't perform any heavyweight processing of them. The second thread performs the processing of the tweets to generate statistics
                    await Task.WhenAll(new List<Task> { ListenToIncomingTweets(tweetReader), ProcessQueuedTweets(loggingInterval) });
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("ERROR - {0} ", e.Message);
            }

        }

        /// <summary>
        ///  Continuously listens on the provided Twitter stream. For every tweet encountered, it is added to a queue to be processed by another thread.
        ///  This is so that the thread performing the collection is not bogged down by processing and can keep up with the flow of tweets from Twitter.
        /// </summary>
        /// <param name="tweetStreamReader">Reference to the Stream Reader object established from the Twitter connection which will contain tweets to listen for</param>
        private async Task ListenToIncomingTweets(StreamReader tweetStreamReader)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    // Read each line (tweet) from the stream. If we encounter a valid line of data, add it to the queue to be processed by the other thread
                    string? tweetJsonStr = tweetStreamReader.ReadLine();
                    if (tweetJsonStr != null && tweetJsonStr != "")
                    {
                        // Add the tweet to the queue to be processed by another thread
                        tweetsToProcess.Enqueue(tweetJsonStr);
                    }
                }
            });
        }

        /// <summary>
        ///  Continuously runs and processes collecting statistics for Tweets that were encountered by the other thread listening for tweets.
        /// </summary>
        /// <param name="loggingInterval">Indicates how often to display collected data to the console (in milliseconds)</param>
        private async Task ProcessQueuedTweets(uint loggingInterval)
        {
            await Task.Run(() =>
            {
                // This section performs the logging to the console every loggingInterval period of time
                DateTime startTime = DateTime.Now;
                System.Timers.Timer loggingTimer = new System.Timers.Timer(loggingInterval);
                Console.WriteLine("Please wait " + loggingInterval / 1000 + " seconds. Collecting first set of data.");
                loggingTimer.Elapsed += (Object? source, ElapsedEventArgs e) =>
                {
                    Console.Clear();
                    Console.WriteLine("Total Tweets between " + startTime.ToString() + " and " + DateTime.Now.ToString() + ": \n" + TotalTweets + "\n");
                    Console.WriteLine("Top Ten Hashtags: ");
                    foreach (var hashtag in TopTenHashtags)
                    {
                        Console.WriteLine(hashtag.Key + " - " + hashtag.Value);
                    }
                };
                loggingTimer.AutoReset = true;
                loggingTimer.Enabled = true;
                loggingTimer.Start();

                // This is the main loop which processes the statistics for the Tweets encountered
                while (true)
                {
                    string? tweetJsonStr = "";

                    // Remove a tweet from the queue so that we can process it
                    if (tweetsToProcess.Count > 0 && tweetsToProcess.TryDequeue(out tweetJsonStr) && tweetJsonStr != "")
                    {
                        // Increment the total count of Tweets processed so far
                        TotalTweets++;

                        // Convert the JSON object Tweet into a C# object so that it can be processed
                        TwitterDataCollector.Tweet? tweet = JsonSerializer.Deserialize<TwitterDataCollector.Tweet>(tweetJsonStr);

                        // Match the text portion of the tweet against the Hashtag Regex to find each occurrence of the Hashtag
                        var foundHashtags = Regex.Matches(tweet?.data?.text ?? "", HASHTAG_REGEX).ToList();
                        foreach (Match foundHashtag in foundHashtags)
                        {
                            // If we haven't encountered this hashtag yet, add it to the Dictionary with 0 count
                            if (!hashtags.ContainsKey(foundHashtag.Value))
                            {
                                hashtags.Add(foundHashtag.Value, 0);
                            }

                            // Increment the amount of times we've seen this hashtag
                            hashtags[foundHashtag.Value]++;
                        }
                    }
                }
            });
        }
    }
}
