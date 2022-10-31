using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using SampledStreamServer.Models;

namespace SampledStreamServer.Services
{
    public class TwitterSampledStreamCapture : BackgroundService, ITwitterEndpointCapture
    {
        private ITwitterConfigFile config;
        private readonly IHttpClientFactory _clientFactory;

        // Contains data that has been captured on this endpoint (but hasn't been processed yet)
        public BlockingCollection<string> capturedData { get; private set; } = new BlockingCollection<string>();

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="config">The configuration file which contains the necessary configuration info to connect to the Twitter endpoint</param>
        ///<param name="_clientFactory">The HTTP Client Factory that will be used to create the connection</param>
        public TwitterSampledStreamCapture(ITwitterConfigFile config, IHttpClientFactory clientFactory)
        {
            this.config = config;
            this._clientFactory = clientFactory;
        }

        public override async Task StartAsync(CancellationToken token)
        {
            await ExecuteAsync(token);
        }

        ///<summary>
        /// Establishes a connection to this Twitter endpoint and continues capturing data until StopAsync is called to stop the capture
        ///</summary>
        ///<returns>A Task that can be awaited to wait for data to capture</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                // Set the Bearer token in the Authorization HTTP Headers
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.userBearerToken);

                // Establish the connection to the Stream
                using (Stream tweetStream = await client.GetStreamAsync(config.sampleStreamURL))
                {
                    StreamReader tweetStreamReader = new StreamReader(tweetStream);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Read each line (tweet) from the stream. If we encounter a valid line of data, add it to the queue to be processed by the consumer
                        string? tweetJsonStr = tweetStreamReader.ReadLine();
                        if (tweetJsonStr != null && tweetJsonStr != "")
                        {
                            // Add the tweet to the queue to be processed later by a processor
                            capturedData.Add(tweetJsonStr);
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
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
