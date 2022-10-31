using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SampledStreamServer.Services
{
    public interface ITwitterEndpointCapture : IHostedService
    {
        // Contains the unprocessed data captured on this endpoint
        public BlockingCollection<string> capturedData { get; }

    }
}
