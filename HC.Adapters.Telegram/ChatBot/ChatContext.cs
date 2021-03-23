using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HC.Adapters.Telegram.ChatBot
{
    public class ChatContext
    {
        private IChatState _state;

        public ChatContext(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
        {
            _state = new UndefinedChatState(chat, telegramBotClient, serviceProvider);
        }

        public async Task Handle(Message message)
        {
            if (message.Text?.StartsWith('/') ?? false)
            {
                _state = await _state.HandleCommand(message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).First(), message);
            }
            else
            {
                _state = await _state.Handle(message);
            }

            await _state.OnStateChanged();
        }

        public async Task Handle(CallbackQuery callbackQuery)
        {
            _state = await _state.HandleCallbackQuery(callbackQuery);

            await _state.OnStateChanged();
        }
    }
}