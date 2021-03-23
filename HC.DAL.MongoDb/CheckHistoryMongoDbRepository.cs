using System.Threading;
using System.Threading.Tasks;
using HC.Domain;
using HC.Domain.Repositories;
using MongoDB.Driver;

namespace HC.DAL.MongoDb
{
    public class CheckHistoryMongoDbRepository : ICheckHistoryRepository
    {
        private readonly IMongoCollection<CheckStatus> _collection;

        public CheckHistoryMongoDbRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<CheckStatus>(nameof(CheckStatus));
        }

        public async Task Store(CheckStatus checkStatus, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(checkStatus, new InsertOneOptions(), cancellationToken);
        }
    }
}