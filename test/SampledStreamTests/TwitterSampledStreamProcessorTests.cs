using System.Collections.Concurrent;
using System.Timers;
using SampledStreamServer.Controllers;
using SampledStreamServer.Models;

namespace SampledStreamTests
{
    public class TwitterSampledStreamProcessorTests
    {
        readonly ConcurrentQueue<string> testData = new (new List<string>
        {
            "{\"data\":{\"edit_history_tweet_ids\":[\"1234\"],\"id\":\"1234\",\"text\":\"This is the #topHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1235\"],\"id\":\"1235\",\"text\":\"I also like #topHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1236\"],\"id\":\"1236\",\"text\":\"#topHashtag is nice but so is #secondHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1237\"],\"id\":\"1237\",\"text\":\"I love #topHashtag but also like #secondHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1238\"],\"id\":\"1238\",\"text\":\"Rather than #topHashtag and #secondHashtag the best is #thirdHashtag\"}}",
        });

        readonly List<KeyValuePair<string, uint>> expectedTopTenHashtagss = new ()
        {
            new KeyValuePair<string, uint>("#topHashtag", 5),
            new KeyValuePair<string, uint>("#secondHashtag", 3),
            new KeyValuePair<string, uint>("#thirdHashtag", 1),
        };

        readonly uint expectedTotalTweets = 5;

        [TestCase(Description="Using above testData should produce expected results")]
        public async Task TwitterSampledStreamProcessorTest()
        {
            ITwitterEndpointProcessor processor = new TwitterSampledStreamProcessor(testData, new RegexHashtagParser());

            // Give a few seconds to process the test data then stop the capture
            System.Timers.Timer? sampledStreamReportingTimer = new(3000);
            sampledStreamReportingTimer.Elapsed += (Object? source, ElapsedEventArgs e) =>
            {
                processor.StopProcessingData();
            };
            sampledStreamReportingTimer.Enabled = true;
            sampledStreamReportingTimer.Start();

            await processor.ProcessCapturedData();

            Assert.That(processor.totalTweets, Is.EqualTo(expectedTotalTweets), "Expected total tweet counts to match");
            Assert.That(processor.topTenHashtags, Is.EquivalentTo(expectedTopTenHashtagss), "Expected top ten hashtags do not match");
        }
    }
}