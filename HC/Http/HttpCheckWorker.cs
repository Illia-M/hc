using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HC.ApplicationServices.Checks.HttpCheck;
using HC.Domain.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HC.Http
{
    public class HttpCheckWorker : BackgroundService
    {
        private readonly HttpCheckExecutor _httpCheckExecutor;
        private readonly IHttpCheckSettingsRepository _httpCheckSettingsRepository;
        private readonly ILogger<HttpCheckWorker> _logger;

        public HttpCheckWorker(HttpCheckExecutor httpCheckExecutor, IHttpCheckSettingsRepository httpCheckSettingsRepository, ILogger<HttpCheckWorker> logger)
        {
            _httpCheckExecutor = httpCheckExecutor ?? throw new ArgumentNullException(nameof(httpCheckExecutor));
            _httpCheckSettingsRepository = httpCheckSettingsRepository ?? throw new ArgumentNullException(nameof(httpCheckSettingsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutTokenSource.Token);
                    var settings = await _httpCheckSettingsRepository.GetAll(cts.Token);
                    var tasks = settings.Select(setting => _httpCheckExecutor.CheckSite(setting, cts.Token));

                    await Task.WhenAll(tasks);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Cancellation requested");
                }
            }
        }
    }
}