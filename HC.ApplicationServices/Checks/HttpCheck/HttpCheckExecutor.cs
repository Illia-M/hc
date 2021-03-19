using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HC.ApplicationServices.History;
using HC.ApplicationServices.Notifications;
using HC.Domain;
using Microsoft.Extensions.Logging;

namespace HC.ApplicationServices.Checks.HttpCheck
{
    public class HttpCheckExecutor
    {
        private readonly HttpClient _client;
        private readonly INotificationService _notificationService;
        private readonly StatusWriter _statusWriter;
        private readonly ILogger<HttpCheckExecutor> _logger;

        public HttpCheckExecutor(HttpClient client, INotificationService notificationService, StatusWriter statusWriter, ILogger<HttpCheckExecutor> logger)
        {
            _notificationService = notificationService;
            _statusWriter = statusWriter;
            _logger = logger;
            _client = client;
        }

        public async Task CheckSite(Domain.HttpCheck httpCheckSettings, CancellationToken cancellationToken)
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

        private async Task TryNotify(Domain.HttpCheck httpCheckSettings, string text, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(httpCheckSettings.TelegramChatId))
            {
                await _notificationService.Notify(httpCheckSettings.TelegramChatId, text, cancellationToken);
            }
        }
    }
}