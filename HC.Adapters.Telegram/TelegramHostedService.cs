using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HC.ApplicationServices.Checks.HttpCheck;
using HC.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HC.Adapters.Telegram
{
    public class TelegramHostedService : IHostedService
    {
        private readonly ConcurrentDictionary<long, bool> _chatsAwaitingCheckAdDictionary = new ConcurrentDictionary<long, bool>();
        private readonly TelegramBotClient? _telegramBotClient;
        private readonly IHttpCheckSettingsRepository _httpCheckSettingsRepository;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<TelegramHostedService> _logger;

        public TelegramHostedService(TelegramBotClient telegramBotClient, IHttpCheckSettingsRepository httpCheckSettingsRepository, IHostApplicationLifetime hostApplicationLifetime, ILogger<TelegramHostedService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _httpCheckSettingsRepository = httpCheckSettingsRepository;
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
                await _httpCheckSettingsRepository.Remove(Guid.Parse(e.CallbackQuery.Data), CancellationToken.None);

                await _telegramBotClient?.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Removed!", cancellationToken: CancellationToken.None);
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
                    await SendHelp(message);
                }
                break;
                case "/list":
                {
                    var userChecks = await _httpCheckSettingsRepository.GetByTelegramId(message.Chat.Id.ToString(), CancellationToken.None);

                    if (!userChecks.Any())
                    {
                        await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Your checks list empty now, try add checks with /addHealthCheck command", ParseMode.Default);
                    }
                    else
                    {
                        foreach (var userCheck in userChecks)
                        {
                            await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, $"{userCheck.Uri}", disableWebPagePreview: true, replyMarkup: new InlineKeyboardMarkup(new[] {
                                InlineKeyboardButton.WithCallbackData("Remove", userCheck.Id.ToString("N")),
                            }));
                        }
                    }
                }
                break;
                case "/addHealthCheck":
                {
                    _chatsAwaitingCheckAdDictionary.AddOrUpdate(message.Chat.Id, true, (s, b) => true);
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Send me some resource, for now i can handle HTTP check only", ParseMode.Default);
                }
                break;
                case "/removeHealthCheck":
                {
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Not implemented yet :|", ParseMode.Default);
                }
                break;
                default:
                {
                    if (_chatsAwaitingCheckAdDictionary.TryRemove(message.Chat.Id, out var isAwaitingAdd) &&
                        isAwaitingAdd)
                    {
                        await HandleAddCheck(message);
                        return;
                    }

                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Can not understand what you want :(", ParseMode.Default);
                    await SendHelp(message);
                }
                break;
            }
        }

        private async Task HandleAddCheck(Message message)
        {
            if (!Uri.TryCreate(message.Text, UriKind.Absolute, out var url))
            {
                await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, "Can not handle this type of resource :(", ParseMode.Default);
                return;
            }

            switch (url.Scheme)
            {
                case "http":
                case "https":

                    var isAdded = await _httpCheckSettingsRepository.Add(new HttpCheck(Guid.NewGuid(), url.ToString(), message.Chat.Id.ToString(),
                        TimeSpan.FromSeconds(30), new ushort[] { 200 }, new Dictionary<string, string>()), CancellationToken.None);

                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id,
                        "Your check " + (isAdded ? "added!" : "not added :("), ParseMode.Default);
                    break;
                default:
                    await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id,
                        "Can not handle this type of resource :(", ParseMode.Default);
                    break;
            }
        }

        private async Task SendHelp(Message message)
        {
            var text = @"I understand next commands:

    /help - display this help :)
    /addHealthCheck - for setup health check to any supported resource
    /removeHealthCheck - for remove one of your health check :'(
    /list - for list of all your health checks";

            await _telegramBotClient!.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Default);
        }
    }
}