using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SampledStreamServer
{
    public class ConsoleReporter : DataReporter
    {
        private System.Timers.Timer? sampledStreamReportingTimer;

        ///<summary>
        /// This function will continuously and periodically report the captured/processed Twitter Sampled Stream data to the user via the console
        ///</summary>
        ///<param name="reportIntervalMs">The interval in milliseconds to report to the user</param>
        ///<param name="processor">The TwitterEndpointProcessor which contains the processed data that will be reported</param>
        public async Task ReportSampledStreamPeriodically(uint reportIntervalMs, TwitterEndpointProcessor processor)
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
