// This program uses the Twitter API v2 Sampled Stream Endpoint to collect a random sample of Tweets
// These tweets are then used to provide summary data such as the number of tweets collected and the top 10 hashtags
// encountered during the collection period.

// Allow Unicode in the console so we can see hashtags from other languages
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Initialize the HTTP Client. We only expect a single client to be initialized in the entire program
HttpClient client = new HttpClient();

const uint consoleReportingIntervalMs = 20000;

try
{
    // Read the configuration file to be passed into the Sampled stream endpoint capturer
    SampledStreamServer.TwitterConfigFile config = new SampledStreamServer.TwitterConfigXml();

    // Initialize the Sampled Stream Endpoint Capturer
    SampledStreamServer.TwitterEndpointCapture capturer = new SampledStreamServer.TwitterSampledStreamCapture(config, client);

    // Initialize the Sampled Stream Endpoint data processor
    SampledStreamServer.TwitterEndpointProcessor processor = new SampledStreamServer.TwitterSampledStreamProcessor(capturer.capturedData, new SampledStreamServer.RegexHashtagParser());

    // Initialize the Data Reporter that will log the captured/processed data to the console
    SampledStreamServer.DataReporter reporter = new SampledStreamServer.ConsoleReporter();

    await Task.WhenAll(new List<Task> { capturer.ConnectAndCapture(), processor.ProcessCapturedData(), reporter.ReportSampledStreamPeriodically(consoleReportingIntervalMs, processor) });
}
catch (Exception ex)
{
    Console.WriteLine("Internal Server Error: " + ex.ToString());
    return -1;
}

return 0;