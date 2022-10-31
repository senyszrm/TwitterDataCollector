// This program uses the Twitter API v2 Sampled Stream Endpoint to collect a random sample of Tweets
// These tweets are then used to provide summary data such as the number of tweets collected and the top 10 hashtags
// encountered during the collection period.

using SampledStreamServer.Models;
using SampledStreamServer.Services;
using SampledStreamServer.Database;

// Allow Unicode in the console so we can see hashtags from other languages
Console.OutputEncoding = System.Text.Encoding.UTF8;

string databasePath = String.Format("{0}sampled_stream.db", AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DI for our service objects
builder.Services.AddTransient(s => new SampledStreamDbContext(databasePath));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITwitterConfigFile, TwitterConfigXml>();
builder.Services.AddSingleton<IHashtagParser, RegexHashtagParser>();
builder.Services.AddSingleton<ITwitterEndpointCapture, TwitterSampledStreamCapture>();
builder.Services.AddSingleton<ITwitterEndpointProcessor, TwitterSampledStreamProcessor>();

// Start the background service which captures and processes the sampled stream endpoint data
builder.Services.AddHostedService<TwitterSampledStreamCaptureAndProcess>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();