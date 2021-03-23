using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HC.Domain;
using MongoDB.Driver;

namespace HC.DAL.MongoDb
{
    public class HttpCheckSettingsMongoDbRepository : IHttpCheckSettingsRepository
    {
        private readonly IMongoCollection<HttpCheck> _collection;

        public HttpCheckSettingsMongoDbRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<HttpCheck>(nameof(HttpCheck));
        }

        public async Task<IReadOnlyCollection<HttpCheck>> GetAll(CancellationToken cancellationToken)
        {
            var results = await _collection.Find(check => true).ToListAsync(cancellationToken: cancellationToken);

            return results.ToArray();
        }

        public async Task<IReadOnlyCollection<HttpCheck>> GetByTelegramId(string telegramChatId, CancellationToken cancellationToken)
        {
            var results = await _collection.Find(settings => settings.TelegramChatId == telegramChatId).ToListAsync();

            return results.ToArray();
        }

        public async Task<bool> Add(HttpCheck newHttpCheckSettings, CancellationToken cancellationToken)
        {
            await _collection.UpdateOneAsync(
                check => check.Id == newHttpCheckSettings.Id,
                new ObjectUpdateDefinition<HttpCheck>(newHttpCheckSettings),
                new UpdateOptions { IsUpsert = true },
                cancellationToken);

            return true;
        }

        public async Task Remove(Guid id, CancellationToken cancellationToken)
        {
            await _collection.DeleteOneAsync(check => check.Id == id, cancellationToken: cancellationToken);
        }
    }
}