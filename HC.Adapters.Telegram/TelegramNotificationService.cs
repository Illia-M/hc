using System;
using System.Threading;
using System.Threading.Tasks;
using HC.ApplicationServices.Notifications;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace HC.Adapters.Telegram
{
    public class TelegramNotificationService : INotificationService
    {
        private readonly TelegramBotClient? _telegramBotClient;
        private readonly ILogger<TelegramNotificationService> _logger;

        public TelegramNotificationService(TelegramBotClient? telegramBotClient, ILogger<TelegramNotificationService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public async Task Notify(string channelId, string text, CancellationToken cancellationToken)
        {
            try
            {
                if (_telegramBotClient is { })
                {
                    await _telegramBotClient.SendTextMessageAsync(channelId, text, ParseMode.Default, disableWebPagePreview: true, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on send notification to Telegram");
            }
        }
    }
}
