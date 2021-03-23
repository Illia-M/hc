using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HC.Adapters.Telegram.ChatBot
{
    public class UndefinedChatState : BaseChatState
    {
        public UndefinedChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override Task OnStateChanged()
        {
            return Task.CompletedTask;
        }
    }
}