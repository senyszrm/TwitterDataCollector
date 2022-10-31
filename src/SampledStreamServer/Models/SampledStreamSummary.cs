namespace SampledStreamServer.Models;

public class SampledStreamSummary
{
    // Returns the Top Ten Hashtags encountered during the collection period
    public List<KeyValuePair<string, uint>> topTenHashtags { get; set; } = new();

    // Stores the total count of tweets processed during the collection period
    public uint totalTweets { get; set; }

}
