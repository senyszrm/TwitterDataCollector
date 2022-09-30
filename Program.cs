// This program uses the Twitter API v2 Sampled Stream Endpoint to collect a random sample of Tweets
// These tweets are then used to provide summary data such as the number of tweets collected and the top 10 hashtags
// encountered during the collection period.

// Allow Unicode in the console so we can see hashtags from other languages
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Create a connection to the Twitter API
TwitterDataCollector.TwitterSampleCollector twitterCollector = new TwitterDataCollector.TwitterSampleCollector();

// Start listening to the Twitter stream and collecting/logging data
await twitterCollector.ConnectAndProcessTweets(20000);