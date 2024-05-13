using Microsoft.Extensions.Options;
using Severis.FileProcessor.Services;

namespace Severis.FileProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher? _step1_folderWatcher;
        private readonly string _step1_inputFolder;
        private readonly string _step2_inputFolder;
        private readonly string _step3_inputFolder;
        private readonly IServiceProvider _services;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> settings, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await Task.CompletedTask;
            await ScanAllFiles(stoppingToken);
        }
        private async Task ScanAllFiles(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is working.");
            var scanDirectories = new List<string> {
                this._step1_inputFolder,this._step2_inputFolder,this._step3_inputFolder};
            using (var scope = this._services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedProcessingService>();
                foreach (var path in scanDirectories.ToArray<string>())
                {
                    //DirectoryInfo dir = new DirectoryInfo(path);
                    await scopedProcessingService.DoWork(stoppingToken, path);
                }

            }
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");
            if (_step1_folderWatcher != null)
            {
                _step1_folderWatcher.EnableRaisingEvents = false;
            }
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _step1_folderWatcher?.Dispose();
            base.Dispose();
        }
    }
}
