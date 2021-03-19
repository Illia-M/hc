using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HC.Adapters.Telegram
{
    public class TelegramHostedService : IHostedService
    {
        private readonly TelegramBotClient? _telegramBotClient;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<TelegramHostedService> _logger;

        public TelegramHostedService(TelegramBotClient telegramBotClient, IHostApplicationLifetime hostApplicationLifetime, ILogger<TelegramHostedService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_telegramBotClient is null)
            {
                _logger.LogInformation("Telegram listening not configured");
                return Task.CompletedTask;
            }

            _telegramBotClient.OnMessage += TgClientOnOnMessage;
            _telegramBotClient.OnMessageEdited += TgClientOnOnMessageEdited;

            _telegramBotClient.StartReceiving(new[] { UpdateType.Message }, _hostApplicationLifetime.ApplicationStopping);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _telegramBotClient?.StopReceiving();

            return Task.CompletedTask;
        }

        private void TgClientOnOnMessageEdited(object? sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message?.Type == MessageType.Text)
            {
                _logger.LogDebug($"{message.Chat.Id}: {message.Text}");
            }
        }

        private async void TgClientOnOnMessage(object? sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message?.Type == MessageType.Text)
                {
                    _logger.LogDebug($"{message.Chat.Id}: {message.Text}");
                    await HandleTextMessage(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on handle new message in chat {ChatId}", messageEventArgs.Message?.Chat?.Id);
            }
        }

        private async Task HandleTextMessage(Message message)
        {
            switch (message.Text)
            {
                case "/start":
                case "/restart":
                {
                    var text =
                        "Hello!\n\nI can help you setup health checks for your resources with notification in Telegram!\n\nSend me /help command for more details!";
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Default);
                }
                    break;
                case "/help":
                {
                    var text = @"I understand next commands:

/help - display this help :)
/addHealthCheck - for setup health check to any supported resource
/removeHealthCheck - for remove one of your health check :'(
/list - for list of all your health checks";

                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Default);
                }
                    break;
                case "/list":
                {
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Not implemented yet :|", ParseMode.Default);
                }
                    break;
                case "/addHealthCheck":
                {
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Not implemented yet :|", ParseMode.Default);
                }
                    break;
                case "/removeHealthCheck":
                {
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Not implemented yet :|", ParseMode.Default);
                }
                    break;
            }
        }
    }
}