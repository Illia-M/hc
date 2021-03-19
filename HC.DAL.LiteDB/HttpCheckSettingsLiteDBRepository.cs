using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HC.Domain;
using LiteDB;
using Microsoft.Extensions.Options;

namespace HC.DAL.LiteDB
{
    public class HttpCheckSettingsLiteDBRepository : IHttpCheckSettingsRepository
    {
        private readonly ILiteCollection<HttpCheck> _collection;

        public HttpCheckSettingsLiteDBRepository(LiteDatabase liteDatabase)
        {
            _collection = liteDatabase.GetCollection<HttpCheck>(nameof(HttpCheck));

            _collection.EnsureIndex(x => x.Uri, true);
        }

        public Task<IReadOnlyCollection<HttpCheck>> GetAll(CancellationToken cancellationToken)
        {
            var results = _collection.FindAll();

            return Task.FromResult<IReadOnlyCollection<HttpCheck>>(results.ToArray());
        }

        public Task<IReadOnlyCollection<HttpCheck>> GetByTelegramId(string telegramChatId, CancellationToken cancellationToken)
        {
            var results = _collection.Find(settings => settings.TelegramChatId == telegramChatId);

            return Task.FromResult<IReadOnlyCollection<HttpCheck>>(results.ToArray());
        }

        public Task<bool> Add(HttpCheck newHttpCheckSettings, CancellationToken cancellationToken)
        {
            _collection.Upsert(newHttpCheckSettings.Id, newHttpCheckSettings);

            return Task.FromResult(true);
        }

        public Task Remove(Guid id, CancellationToken cancellationToken)
        {
            _collection.Delete(id);

            return Task.CompletedTask;
        }
    }
}
