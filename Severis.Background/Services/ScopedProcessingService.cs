using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Severis.FileProcessor.Services
{
    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken,string path);
    }

    internal class ScopedProcessingService : IScopedProcessingService
    {
        private readonly ILogger _logger;

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task DoWork(CancellationToken stoppingToken, string path)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    $"Scan all files in the path: {path}");

                await Task.CompletedTask;
            }
        }
    }
}
