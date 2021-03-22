using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HC.Adapters.Telegram
{
    public class UnknownCommandChatState : BaseChatState
    {
        public UnknownCommandChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override Task OnStateChanged()
        {
            throw new NotImplementedException();
        }
    }
}