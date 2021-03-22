using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HC.Adapters.Telegram
{
    public abstract class BaseChatState : IChatState
    {
        protected readonly TelegramBotClient TelegramBotClient;
        protected readonly IServiceProvider ServiceProvider;

        public BaseChatState(TelegramBotClient telegramBotClient, IServiceProvider serviceProvider, Chat chat)
        {
            TelegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Chat = chat ?? throw new ArgumentNullException(nameof(chat));
        }

        public Chat Chat {
            get;
        }

        virtual public Task<IChatState> Handle(Message message)
        {
            return Task.FromResult<IChatState>(this);
        }

        public virtual async Task<IChatState> HandleCommand(string command, Message message)
        {
            return command switch {
                "/start" => new StartChatState(Chat, TelegramBotClient, ServiceProvider),
                "/help" => new HelpChatState(Chat, TelegramBotClient, ServiceProvider),
                "/list" => new ListHealthChecksChatState(Chat, TelegramBotClient, ServiceProvider),
                "/addHealthCheck" => new AddHealthChecksChatState(Chat, TelegramBotClient, ServiceProvider),
                //"/removeHealthCheck" => new RemoveHealthChecksChatState(Chat, TelegramBotClient, ServiceProvider),
                _ => new UnknownCommandChatState(Chat, TelegramBotClient, ServiceProvider)
            };
        }

        public virtual async Task<IChatState> HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            await TelegramBotClient.AnswerCallbackQueryAsync(callbackQuery.Id, null, cancellationToken: CancellationToken.None);

            return this;
        }

        public abstract Task OnStateChanged();

        protected async Task SendMessage(string message, bool disableWebPagePreview = false, IReplyMarkup replyMarkup = null,
            ParseMode parseMode = ParseMode.Default)
        {
            await TelegramBotClient.SendTextMessageAsync(Chat.Id, message, replyMarkup: replyMarkup, parseMode: parseMode, disableWebPagePreview: disableWebPagePreview);
        }
    }
}