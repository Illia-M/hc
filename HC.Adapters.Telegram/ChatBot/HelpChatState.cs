using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HC.Adapters.Telegram
{
    public class HelpChatState : BaseChatState
    {
        public HelpChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override Task OnStateChanged()
        {
            var text = @"I understand next commands:

    /help - display this help :)
    /addHealthCheck - for setup health check to any supported resource
    /removeHealthCheck - for remove one of your health check :'(
    /list - for list of all your health checks";

            return base.SendMessage(text);
        }
    }
}