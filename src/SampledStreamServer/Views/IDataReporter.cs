using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampledStreamServer.Controllers;

namespace SampledStreamServer.Views
{
    public interface IDataReporter
    {
        ///<summary>
        /// This function will continuously and periodically report the captured/processed Twitter Sampled Stream data to the user
        ///</summary>
        public Task ReportSampledStreamPeriodically();

        ///<summary>
        /// Stops any currently in progress sampled stream periodic report
        ///</summary>
        public void StopSampledStreamPeriodicReport();
    }
}
