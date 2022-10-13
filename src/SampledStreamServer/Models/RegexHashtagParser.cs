using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SampledStreamServer.Models
{
    public class RegexHashtagParser : IHashtagParser
    {
        // This Regex is used to match hashtags. Processes alphanumeric Unicode characters so Hashtags from other languages will be present
        private const string HASHTAG_REGEX = @"(#+[\p{L}\p{N}(_)]{1,})";

        ///<summary>
        /// Given a string input containing zero or more hashtags, will parse and return a list of hashtags found within the string
        ///</summary>
        ///<param name="input">The input string to parse</param>
        ///<returns>A list of hashtags found within the input string</returns>
        public List<string> Parse(string input)
        {
            // Match the text portion of the tweet against the Hashtag Regex to find each occurrence of a Hashtag
            return Regex.Matches(input ?? "", HASHTAG_REGEX).Select((x) => x.Value).ToList();
        }
    }
}
