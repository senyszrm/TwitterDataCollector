namespace SampledStreamServer.Services
{
    public class TwitterSampledStreamCaptureAndProcess : BackgroundService
    {
        private ITwitterEndpointCapture capturer;
        private ITwitterEndpointProcessor processor;

        public TwitterSampledStreamCaptureAndProcess(ITwitterEndpointCapture capturer, ITwitterEndpointProcessor processor)
        {
            this.capturer = capturer;
            this.processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(capturer.StartAsync(stoppingToken), processor.StartAsync(stoppingToken));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(capturer.StopAsync(cancellationToken), processor.StopAsync(cancellationToken));
        }
    }
}
