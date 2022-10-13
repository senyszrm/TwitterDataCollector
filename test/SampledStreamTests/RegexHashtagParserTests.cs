using SampledStreamServer.Models;

namespace SampledStreamTests
{
    public class RegexParserTests
    {

        [TestCase("", new string[] {}, Description = "Empty String should return empty list of hashtags")]
        [TestCase("This is my #tweet", new string[] { "#tweet"}, Description = "Should match single hashtag at the end of a tweet")]
        [TestCase("#This #is #my #tweet", new string[] { "#This", "#is", "#my", "#tweet" }, Description = "Should match hashtags in every position")]
        [TestCase("#007 Test numeric", new string[] { "#007" }, Description = "Should match a numeric hashtag")]
        public void RegexParserTest(string input, string[] expectedOutput)
        {
            List<string> expectedOutputList = expectedOutput.ToList();
            IHashtagParser hashtagParser = new RegexHashtagParser();
            List<string> actualOutput = hashtagParser.Parse(input);

            Assert.That(actualOutput, Is.EquivalentTo(expectedOutputList), "Actual hashtag output does not match expected");
        }
    }
}