using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using SampledStreamServer.Models;

namespace SampledStreamServer.Controllers
{
    public class TwitterSampledStreamCapture : ITwitterEndpointCapture
    {
        private ITwitterConfigFile config;
        private HttpClient client;
        private bool captureInProgress = false;

        // Contains data that has been captured on this endpoint (but hasn't been processed yet)
        public ConcurrentQueue<string> capturedData { get; private set; } = new ConcurrentQueue<string>();

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="config">The configuration file which contains the necessary configuration info to connect to the Twitter endpoint</param>
        ///<param name="client">The HTTP Client that will be used to establish the connection</param>
        public TwitterSampledStreamCapture(ITwitterConfigFile config, HttpClient client)
        {
            this.config = config;
            this.client = client;
        }

        ///<summary>
        /// Establishes a connection to this Twitter endpoint and continues capturing data until StopCapture is called to stop the capture
        ///</summary>
        ///<returns>A Task that can be awaited to wait for data to capture</returns>
        public async Task ConnectAndCapture()
        {
            try
            {
                // Set the Bearer token in the Authorization HTTP Headers
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.userBearerToken);

                // Establish the connection to the Stream
                using (Stream tweetStream = await client.GetStreamAsync(config.sampleStreamURL))
                {
                    captureInProgress = true;
                    StreamReader tweetStreamReader = new StreamReader(tweetStream);

                    while (captureInProgress)
                    {
                        // Read each line (tweet) from the stream. If we encounter a valid line of data, add it to the queue to be processed by the consumer
                        string? tweetJsonStr = tweetStreamReader.ReadLine();
                        if (tweetJsonStr != null && tweetJsonStr != "")
                        {
                            // Add the tweet to the queue to be processed later by a processor
                            capturedData.Enqueue(tweetJsonStr);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Connection error while trying to capture tweets: {0} ", e.Message);
                throw e;
            }
        }

        ///<summary>
        /// Stops a currently in progress capture
        ///</summary>
        public void StopCapture()
        {
            captureInProgress = false;
        }
    }
}
