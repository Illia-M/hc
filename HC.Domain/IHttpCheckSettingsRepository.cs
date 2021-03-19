using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HC.Domain
{
    public interface IHttpCheckSettingsRepository
    {
        Task<IReadOnlyCollection<HttpCheck>> GetAll(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<HttpCheck>> GetByTelegramId(string telegramChatId, CancellationToken cancellationToken);
        Task<bool> Add(HttpCheck newHttpCheckSettings, CancellationToken cancellationToken);
        Task Remove(Guid id, CancellationToken cancellationToken);
    }
}