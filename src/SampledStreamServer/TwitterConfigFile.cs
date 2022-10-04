using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampledStreamServer
{
    public interface TwitterConfigFile
    {
        // Contains the URL to the sample stream endpoint obtained from the config file
        public string sampleStreamURL { get; }
        // Contains the user's bearer token obtained from the config file
        public string userBearerToken { get; }
    }
}
