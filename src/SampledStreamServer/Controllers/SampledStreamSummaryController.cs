using Microsoft.AspNetCore.Mvc;
using SampledStreamServer.Database;
using SampledStreamServer.Models;

namespace SampledStreamServer.Controllers;

[ApiController]
[Route("[controller]")]
public class SampledStreamSummaryController : ControllerBase
{
    private readonly ILogger<SampledStreamSummaryController> _logger;
    private readonly SampledStreamDbContext _dbContext;

    public SampledStreamSummaryController(ILogger<SampledStreamSummaryController> logger, SampledStreamDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet(Name = "GetSampledStreamSummary")]
    public SampledStreamSummary Get()
    {
        SampledStreamSummary result = new ();
        result.totalTweets = _dbContext.SummaryData.TotalTweets;
        result.topTenHashtags = _dbContext.SummaryData.GetTopTenHashtags(_dbContext) ?? new List<KeyValuePair<string, uint>>();
        return result;
    }
}
