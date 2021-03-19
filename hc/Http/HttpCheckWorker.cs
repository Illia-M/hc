using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HC.ApplicationServices.History;
using HC.ApplicationServices.Notifications;
using HC.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HC.Http
{
    public class HttpCheckWorker : BackgroundService
    {
        private readonly HttpClient _client;
        private readonly INotificationService _notificationService;
        private readonly StatusWriter _statusWriter;
        private readonly IOptions<AppSettings> _options;
        private readonly ILogger<HttpCheckWorker> _logger;

        public HttpCheckWorker(INotificationService notificationService, StatusWriter statusWriter, IOptions<AppSettings> options, ILogger<HttpCheckWorker> logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _statusWriter = statusWriter;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                    using var cts =
                        CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutTokenSource.Token);

                    var tasks = _options.Value.HttpChecks.Select(settings => CheckSite(settings, cts.Token));

                    await Task.WhenAll(tasks);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Cancellation requested");
                }
            }
        }

        public override void Dispose()
        {
            _client.Dispose();
            base.Dispose();
        }

        private async Task CheckSite(HttpCheckSettings httpCheckSettings, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, httpCheckSettings.Uri);

                foreach (var header in httpCheckSettings.Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var sw = Stopwatch.StartNew();
                var result = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, new CancellationTokenSource(httpCheckSettings.Timeout).Token);

                sw.Stop();

                if (httpCheckSettings.SuccessStatusCodes.Contains((ushort)result.StatusCode))
                {
                    _logger.LogInformation("[{Uri}] Alive.\t\tElapsed: {Elapsed} ms", httpCheckSettings.Uri, sw.ElapsedMilliseconds);
                    await _statusWriter.Store(new CheckStatus(httpCheckSettings.Uri!, true, sw.ElapsedMilliseconds, DateTimeOffset.UtcNow), cancellationToken);
                }
                else
                {
                    _logger.LogError("[{Uri}] NOT alive. Status code {StatusCode}.\t\tElapsed: {Elapsed} ms", httpCheckSettings.Uri, result.StatusCode, sw.ElapsedMilliseconds);

                    if (!string.IsNullOrEmpty(httpCheckSettings.TelegramChatId))
                    {
                        await TryNotify(httpCheckSettings, $"[{httpCheckSettings.Uri}] Request failed", cancellationToken);
                    }

                    await _statusWriter.Store(new CheckStatus(httpCheckSettings.Uri!, false, sw.ElapsedMilliseconds, DateTimeOffset.UtcNow), cancellationToken);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
            {
                _logger.LogError(ex, "[{Uri}] Error occurred", httpCheckSettings.Uri);

                if (!string.IsNullOrEmpty(httpCheckSettings.TelegramChatId))
                {
                    await TryNotify(httpCheckSettings, $"‼️ [{httpCheckSettings.Uri}] Error on check: {ex.Message}", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Uri}] Unexpected error occurred", httpCheckSettings.Uri);

                await TryNotify(httpCheckSettings, $"‼️‼️‼️ [{httpCheckSettings.Uri}] Error on check: {ex.Message}", cancellationToken);
            }
        }

        private async Task TryNotify(HttpCheckSettings httpCheckSettings, string text, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(httpCheckSettings.TelegramChatId))
            {
                await _notificationService.Notify(httpCheckSettings.TelegramChatId, text, cancellationToken);
            }
        }
    }
}