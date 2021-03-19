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

        public TelegramNotificationService(TelegramBotClient telegramBotClient, ILogger<TelegramNotificationService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public Task Notify(string channelId, string text, CancellationToken cancellationToken)
        {
            return _telegramBotClient?.SendTextMessageAsync(channelId, text, ParseMode.Default, cancellationToken: cancellationToken) ?? Task.CompletedTask;
        }
    }
}
