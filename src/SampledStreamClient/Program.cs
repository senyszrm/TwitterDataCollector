using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampledStreamClient.Views;

// Allow Unicode in the console so we can see hashtags from other languages
Console.OutputEncoding = System.Text.Encoding.UTF8;

using IHost host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddHttpClient();
    services.AddHostedService<ConsoleReporter>();
}).Build();

await host.RunAsync();