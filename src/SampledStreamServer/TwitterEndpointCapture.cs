using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SampledStreamServer
{
    public interface TwitterEndpointCapture
    {
        // Contains the unprocessed data captured on this endpoint
        public ConcurrentQueue<string> capturedData { get; }

        ///<summary>
        /// Establishes a connection to this Twitter endpoint and continues capturing data until StopCapture is called to stop the capture
        ///</summary>
        ///<returns>A Task that can be awaited to wait for data to capture</returns>
        public Task ConnectAndCapture();

        ///<summary>
        /// Stops a currently in progress capture
        ///</summary>
        public void StopCapture();
    }
}
