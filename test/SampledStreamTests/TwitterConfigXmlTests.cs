using SampledStreamServer.Models;

namespace SampledStreamTests
{
    public class TwitterConfigXmlTests
    {

        [TestCase("ValidConnectionFile.xml", null, new string[] { "https://api.twitter.com/2/tweets/sample/stream", "1234" }, Description = "Valid connection file should set data as expected")]
        [TestCase("InvalidConnectionFile.xml", typeof(InvalidOperationException), new string[] { }, Description = "Invalid connection file should throw InvalidOperation exception")]
        [TestCase("InvalidPath.xml", typeof(FileNotFoundException), new string[] { }, Description = "Invalid path should throw FileNotFound exception")]
        public void TwitterConfigXmlTest(string configXmlPath, System.Type? expectedExceptionType, string[] expectedOutput)
        {
            if (expectedExceptionType != null)
            {
                Assert.Throws(expectedExceptionType, () => new TwitterConfigXml(configXmlPath), "Expected parsing of the xml file to fail");
                return;
            }
            Assert.DoesNotThrow(() => new TwitterConfigXml(configXmlPath), "Expected parsing of the xml file to succeed");

            string expectedStreamURL = expectedOutput[0];
            string expectedBearerToken = expectedOutput[1];
            TwitterConfigXml twitterConfigXml = new TwitterConfigXml(configXmlPath);

            Assert.That(twitterConfigXml.sampleStreamURL, Is.EquivalentTo(expectedStreamURL), "Actual sampled stream URL does not match expected");
            Assert.That(twitterConfigXml.userBearerToken, Is.EquivalentTo(expectedBearerToken), "Actual bearer token does not match expected");
        }
    }
}