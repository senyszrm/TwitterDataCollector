using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Http.Headers;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using SampledStreamServer.Models;

namespace SampledStreamClient.Views
{
    public class ConsoleReporter : BackgroundService
    {
        private System.Timers.Timer? sampledStreamReportingTimer;
        private readonly IHttpClientFactory _clientFactory;
        
        public uint reportIntervalMs { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        ///<param name="reportIntervalMs">The interval in milliseconds to report to the user</param>
        ///<param name="clientFactory">HTTP Client Factory to create a connection to the restApi endpoint</param>
        ///
        public ConsoleReporter(IHttpClientFactory clientFactory, uint reportIntervalMs = 20000)
        {
            this.reportIntervalMs = reportIntervalMs;
            this._clientFactory = clientFactory;
        }

        ///<summary>
        /// This function will continuously and periodically report the captured/processed Twitter Sampled Stream data to the user via the console
        ///</summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                HttpClient client = _clientFactory.CreateClient();

                // This section performs the logging to the console every reportIntervalMs period of time
                sampledStreamReportingTimer = new System.Timers.Timer(reportIntervalMs);
                Console.Clear();
                Console.WriteLine("Please wait " + reportIntervalMs / 1000 + " seconds. Collecting first set of data.");
                sampledStreamReportingTimer.Elapsed += async (Object? source, ElapsedEventArgs e) =>
                {
                    // Connect to the local restApi endpoint to get the captured summary data from our server
                    SampledStreamSummary summaryData = await client.GetFromJsonAsync<SampledStreamSummary>("https://localhost:7111/SampledStreamSummary") ?? new SampledStreamSummary();

                    // Log results to the console
                    Console.Clear();
                    Console.WriteLine("Total Tweets as of " + DateTime.Now.ToString() + ": \n" + summaryData.totalTweets + "\n");
                    Console.WriteLine("Top Ten Hashtags: ");
                    foreach (var hashtag in summaryData.topTenHashtags)
                    {
                        Console.WriteLine(hashtag.Key + " - " + hashtag.Value);
                    }
                };
                sampledStreamReportingTimer.AutoReset = true;
                sampledStreamReportingTimer.Enabled = true;
                sampledStreamReportingTimer.Start();
            });
        }

        ///<summary>
        /// Stops any currently in progress sampled stream periodic report
        ///</summary>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (sampledStreamReportingTimer != null)
            {
                sampledStreamReportingTimer.Stop();
            }

            return Task.CompletedTask;
        }
    }
}
