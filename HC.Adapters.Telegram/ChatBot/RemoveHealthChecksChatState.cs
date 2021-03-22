using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Domain;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HC.Adapters.Telegram
{
    public class RemoveHealthChecksChatState : BaseChatState
    {

        public RemoveHealthChecksChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override async Task<IChatState> HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            var httpCheckSettingsRepository = ServiceProvider.GetRequiredService<IHttpCheckSettingsRepository>();

            await httpCheckSettingsRepository.Remove(Guid.Parse(callbackQuery.Data), CancellationToken.None);

            await base.HandleCallbackQuery(callbackQuery);

            return new UndefinedChatState(Chat, TelegramBotClient, ServiceProvider);
        }

        public override Task OnStateChanged()
        {
            throw new NotImplementedException();
        }
    }
}