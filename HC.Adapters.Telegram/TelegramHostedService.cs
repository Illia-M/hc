using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HC.Adapters.Telegram.ChatBot;
using HC.ApplicationServices.Checks.HttpCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = HC.Adapters.Telegram.ChatBot.Chat;

namespace HC.Adapters.Telegram
{
    public class TelegramHostedService : IHostedService
    {
        private readonly ConcurrentDictionary<string, ChatContext> _chatsDictionary = new ConcurrentDictionary<string, ChatContext>();
        private readonly TelegramBotClient? _telegramBotClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<TelegramHostedService> _logger;

        public TelegramHostedService(TelegramBotClient telegramBotClient, IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime, ILogger<TelegramHostedService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _serviceProvider = serviceProvider;
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
            _telegramBotClient.OnCallbackQuery += TelegramBotClientOnOnCallbackQuery;

            _telegramBotClient.StartReceiving(new[] { UpdateType.Message, UpdateType.CallbackQuery }, _hostApplicationLifetime.ApplicationStopping);

            return Task.CompletedTask;
        }

        private async void TelegramBotClientOnOnCallbackQuery(object? sender, CallbackQueryEventArgs e)
        {
            try
            {
                var chat = Chat.Create(e.CallbackQuery.From.Id);
                var chatContext = _chatsDictionary.GetOrAdd(chat.Id,
                    l => new ChatContext(chat, _telegramBotClient!,
                        _serviceProvider.GetRequiredService<IServiceProvider>()));

                await chatContext.Handle(e.CallbackQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on handle CallbackQuery in chat {ChatId}", e.CallbackQuery?.ChatInstance);
            }
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
            var chat = Chat.Create(message.Chat.Id);
            var chatContext = _chatsDictionary.GetOrAdd(chat.Id,
                l => new ChatContext(chat, _telegramBotClient!,
                    _serviceProvider.GetRequiredService<IServiceProvider>()));

            await chatContext.Handle(message);
        }
    }
}