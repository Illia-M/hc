using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HC.Adapters.Telegram.ChatBot
{
    public class StartChatState : BaseChatState
    {
        public StartChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override async Task OnStateChanged()
        {
            var text =
                "Hello!\n\nI can help you setup health checks for your resources with notification in Telegram!\n\nSend me /help command for more details!";
            await SendMessage(text);
        }
    }
}