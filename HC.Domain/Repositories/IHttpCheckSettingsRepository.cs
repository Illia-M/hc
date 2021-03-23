using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HC.Domain.Repositories
{
    public interface IHttpCheckSettingsRepository
    {
        Task<IReadOnlyCollection<HttpCheck.HttpCheck>> GetAll(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<HttpCheck.HttpCheck>> GetByTelegramId(string telegramChatId, CancellationToken cancellationToken);
        Task<bool> Add(HttpCheck.HttpCheck newHttpCheckSettings, CancellationToken cancellationToken);
        Task Remove(Guid id, CancellationToken cancellationToken);
    }
}