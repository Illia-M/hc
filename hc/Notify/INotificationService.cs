using System.Threading;
using System.Threading.Tasks;

namespace hc.Notify
{
    public interface INotificationService
    {
        Task Notify(string channelId, string text, CancellationToken cancellationToken);
    }
}