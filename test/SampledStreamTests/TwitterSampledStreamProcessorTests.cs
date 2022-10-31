using System.Collections.Concurrent;
using System.Timers;
using SampledStreamServer.Services;
using SampledStreamServer.Database;
using SampledStreamServer.Models;

namespace SampledStreamTests
{
    public class TwitterSampledStreamProcessorTests
    {
        private static readonly string databaseFileName = "sampled_stream";
        private static readonly string pathToTemplateDb = String.Format("..\\..\\..\\..\\..\\src\\SampledStreamServer\\{0}.db", databaseFileName);
        private static readonly string testDatabasePathFormat = ".\\{0}_{1}.db";

        readonly List<string> testData = new ()
        {
            "{\"data\":{\"edit_history_tweet_ids\":[\"1234\"],\"id\":\"1234\",\"text\":\"This is the #topHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1235\"],\"id\":\"1235\",\"text\":\"I also like #topHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1236\"],\"id\":\"1236\",\"text\":\"#topHashtag is nice but so is #secondHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1237\"],\"id\":\"1237\",\"text\":\"I love #topHashtag but also like #secondHashtag\"}}",
            "{\"data\":{\"edit_history_tweet_ids\":[\"1238\"],\"id\":\"1238\",\"text\":\"Rather than #topHashtag and #secondHashtag the best is #thirdHashtag\"}}",
        };

        readonly List<KeyValuePair<string, uint>> expectedTopTenHashtagss = new ()
        {
            new KeyValuePair<string, uint>("#topHashtag", 5),
            new KeyValuePair<string, uint>("#secondHashtag", 3),
            new KeyValuePair<string, uint>("#thirdHashtag", 1),
        };

        readonly uint expectedTotalTweets = 5;

        [SetUp]
        public void SetUp()
        {
            // Copy the template database to be used as a new test database for the test
            File.Copy(pathToTemplateDb, String.Format(testDatabasePathFormat, databaseFileName, TestContext.CurrentContext.Test.ID), true);
        }

        [TestCase(Description="Using above testData should produce expected results")]
        public async Task TwitterSampledStreamProcessorTest()
        {
            using (SampledStreamDbContext sampledStreamDb = new (String.Format(testDatabasePathFormat, databaseFileName, TestContext.CurrentContext.Test.ID)))
            {
                BlockingCollection<string> testBlockingCollection = new ();
                foreach(var item in testData)
                {
                    testBlockingCollection.Add(item);
                }
                ITwitterEndpointProcessor processor = new TwitterSampledStreamProcessor(testBlockingCollection, new RegexHashtagParser(), sampledStreamDb);

                // Give a few seconds to process the test data then stop the capture
                System.Timers.Timer? sampledStreamReportingTimer = new(3000);
                CancellationTokenSource source = new ();
                sampledStreamReportingTimer.Elapsed += (Object? s, ElapsedEventArgs e) =>
                {
                    source.Cancel();
                };
                sampledStreamReportingTimer.Enabled = true;
                sampledStreamReportingTimer.Start();

                await processor.StartAsync(source.Token);

                Assert.That(processor.totalTweets, Is.EqualTo(expectedTotalTweets), "Expected total tweet counts to match");
                Assert.That(processor.topTenHashtags, Is.EquivalentTo(expectedTopTenHashtagss), "Expected top ten hashtags do not match");
            }
        }
    }
}