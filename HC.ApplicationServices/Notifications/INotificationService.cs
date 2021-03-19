using System.Threading;
using System.Threading.Tasks;

namespace HC.ApplicationServices.Notifications
{
    public interface INotificationService
    {
        Task Notify(string channelId, string text, CancellationToken cancellationToken);
    }
}