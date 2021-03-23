using System.Threading;
using System.Threading.Tasks;

namespace HC.Domain.Repositories
{
    public interface ICheckHistoryRepository
    {
        Task Store(CheckStatus checkStatus, CancellationToken cancellationToken);
    }
}