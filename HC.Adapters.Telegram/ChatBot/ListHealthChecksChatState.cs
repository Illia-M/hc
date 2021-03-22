using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HC.Domain;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HC.Adapters.Telegram
{
    public class ListHealthChecksChatState : BaseChatState
    {
        public ListHealthChecksChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override async Task OnStateChanged()
        {
            var httpCheckSettingsRepository = ServiceProvider.GetRequiredService<IHttpCheckSettingsRepository>();
            var userChecks = await httpCheckSettingsRepository.GetByTelegramId(Chat.Id, CancellationToken.None);

            if (!userChecks.Any())
            {
                await SendMessage("Your checks list empty now, try add checks with /addHealthCheck command");
            }
            else
            {
                foreach (var userCheck in userChecks)
                {
                    var callback = BuildCallback("remove", userCheck.Id.ToString("N"));
                    await SendMessage($"{userCheck.Uri}", disableWebPagePreview: true, replyMarkup: new InlineKeyboardMarkup(new[] {
                        InlineKeyboardButton.WithCallbackData("Remove",callback ),
                    }));
                }
            }
        }

        private string BuildCallback(string action, string data)
        {
            var callbackData = $"{action}::{data}";

            if (callbackData.Length > 64)
            {
                throw new InvalidOperationException("Callback data too long, allowed only 64 bytes");
            }

            return callbackData;
        }

        public override async Task<IChatState> HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            IChatState nextState = null;
            var httpCheckSettingsRepository = ServiceProvider.GetRequiredService<IHttpCheckSettingsRepository>();
            var action = callbackQuery.Data.Split("::").FirstOrDefault();

            switch (action)
            {
                case "remove":
                    await httpCheckSettingsRepository.Remove(Guid.Parse(callbackQuery.Data.Substring(callbackQuery.Data.IndexOf("::") + 2)), CancellationToken.None);

                    nextState = this;
                    break;
            }

            await base.HandleCallbackQuery(callbackQuery);

            return nextState ?? new UndefinedChatState(Chat, TelegramBotClient, ServiceProvider);
        }
    }
}