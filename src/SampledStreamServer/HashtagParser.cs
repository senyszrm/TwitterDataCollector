using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampledStreamServer
{
    public interface HashtagParser
    {

        ///<summary>
        /// Given a string input containing zero or more hashtags, will parse and return a list of hashtags found within the string
        ///</summary>
        ///<param name="input">The input string to parse</param>
        ///<returns>A list of hashtags found within the input string</returns>
        List<string> Parse(string input);
    }
}
