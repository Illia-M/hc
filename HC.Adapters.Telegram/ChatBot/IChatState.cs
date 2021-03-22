using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HC.Adapters.Telegram
{
    public interface IChatState
    {
        Task<IChatState> Handle(Message message);

        Task<IChatState> HandleCommand(string command, Message message);

        Task<IChatState> HandleCallbackQuery(CallbackQuery callbackQuery);

        Task OnStateChanged();
    }
}