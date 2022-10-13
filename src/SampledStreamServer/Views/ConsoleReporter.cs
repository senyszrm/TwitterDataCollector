using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SampledStreamServer.Controllers;

namespace SampledStreamServer.Views
{
    public class ConsoleReporter : IDataReporter
    {
        private System.Timers.Timer? sampledStreamReportingTimer;
        ITwitterEndpointProcessor processor;
        public uint reportIntervalMs { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        ///<param name="processor">The TwitterEndpointProcessor which contains the processed data that will be reported</param>
        ///<param name="reportIntervalMs">The interval in milliseconds to report to the user</param>
        ///
        public ConsoleReporter(ITwitterEndpointProcessor processor, uint reportIntervalMs)
        {
            this.processor = processor;
            this.reportIntervalMs = reportIntervalMs;
        }

        ///<summary>
        /// This function will continuously and periodically report the captured/processed Twitter Sampled Stream data to the user via the console
        ///</summary>
        public async Task ReportSampledStreamPeriodically()
        {
            await Task.Run(() =>
            {
                // This section performs the logging to the console every reportIntervalMs period of time
                DateTime startTime = DateTime.Now;
                sampledStreamReportingTimer = new System.Timers.Timer(reportIntervalMs);
                Console.WriteLine("Please wait " + reportIntervalMs / 1000 + " seconds. Collecting first set of data.");
                sampledStreamReportingTimer.Elapsed += (Object? source, ElapsedEventArgs e) =>
                {
                    Console.Clear();
                    Console.WriteLine("Total Tweets between " + startTime.ToString() + " and " + DateTime.Now.ToString() + ": \n" + processor.totalTweets + "\n");
                    Console.WriteLine("Top Ten Hashtags: ");
                    foreach (var hashtag in processor.topTenHashtags)
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
        public void StopSampledStreamPeriodicReport()
        {
            if (sampledStreamReportingTimer != null)
            {
                sampledStreamReportingTimer.Stop();
            }
        }
    }
}
