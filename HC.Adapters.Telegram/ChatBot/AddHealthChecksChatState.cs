using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HC.Domain.HttpCheck;
using HC.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HC.Adapters.Telegram.ChatBot
{
    public class AddHealthChecksChatState : BaseChatState
    {
        public AddHealthChecksChatState(Chat chat, TelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
            : base(telegramBotClient, serviceProvider, chat)
        {
        }

        public override async Task<IChatState> Handle(Message message)
        {
            if (!Uri.TryCreate(message.Text, UriKind.Absolute, out var url))
            {
                await SendMessage("Can not handle this type of resource :(");
                return this;
            }

            switch (url.Scheme)
            {
                case "http":
                case "https":

                    var httpCheckSettingsRepository = ServiceProvider.GetRequiredService<IHttpCheckSettingsRepository>();
                    var isAdded = await httpCheckSettingsRepository.Add(new HttpCheck(Guid.NewGuid(), url.ToString(), message.Chat.Id.ToString(),
                        TimeSpan.FromSeconds(30), new ushort[] { 200 }, new Dictionary<string, string>()), CancellationToken.None);

                    await SendMessage("Your check " + (isAdded ? "added!" : "not added :("));
                    return new UndefinedChatState(Chat, TelegramBotClient, ServiceProvider);
                default:
                    await SendMessage("Can not handle this type of resource :(");
                    return this;
            }
        }

        public override Task OnStateChanged()
        {
            return SendMessage("Send me some resource, for now i can handle HTTP check only");
        }
    }
}