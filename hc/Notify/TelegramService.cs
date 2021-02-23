using System.Threading;
using System.Threading.Tasks;
using hc.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace hc.Notify
{
    public class TelegramService : INotificationService
    {
        private readonly ILogger<TelegramService> _logger;
        private readonly TelegramBotClient? _tgClient;

        public TelegramService(IOptions<AppSettings> options, ILogger<TelegramService> logger)
        {
            _logger = logger;
            if (!string.IsNullOrEmpty(options.Value.Telegram?.Token))
            {
                _tgClient = new TelegramBotClient(options.Value.Telegram.Token);
                _tgClient.OnMessage += TgClientOnOnMessage;
                _tgClient.OnMessageEdited += TgClientOnOnMessageEdited;
            }
        }

        public Task Notify(string channelId, string text, CancellationToken cancellationToken)
        {
            return _tgClient?.SendTextMessageAsync(channelId, text, ParseMode.Default, cancellationToken: cancellationToken) ?? Task.CompletedTask;
        }

        private void TgClientOnOnMessageEdited(object? sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message?.Type == MessageType.Text)
            {
                _logger.LogDebug($"{message.Chat.Id}: {message.Text}");
            }
        }

        private void TgClientOnOnMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message?.Type == MessageType.Text)
            {
                _logger.LogDebug($"{message.Chat.Id}: {message.Text}");
            }
        }
    }
}
