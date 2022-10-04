using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampledStreamServer
{
    public interface DataReporter
    {
        ///<summary>
        /// This function will continuously and periodically report the captured/processed Twitter Sampled Stream data to the user
        ///</summary>
        ///<param name="reportIntervalMs">The interval in milliseconds to report to the user</param>
        ///<param name="processor">The TwitterEndpointProcessor which contains the processed data that will be reported</param>
        public Task ReportSampledStreamPeriodically(uint reportIntervalMs, TwitterEndpointProcessor processor);

        ///<summary>
        /// Stops any currently in progress sampled stream periodic report
        ///</summary>
        public void StopSampledStreamPeriodicReport();
    }
}
