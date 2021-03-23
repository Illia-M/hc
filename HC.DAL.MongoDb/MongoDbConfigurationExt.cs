using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HC.DAL.MongoDb
{
    public static class MongoDbConfigurationExt
    {
        public static bool CanConfigureMongoDbStorage(this IConfiguration configuration)
        {
            return !string.IsNullOrWhiteSpace(configuration.GetSection(nameof(MongoDbSettings))?[nameof(MongoDbSettings.ConnectionString)]);
        }

        public static IServiceCollection AddMongoDbDal(this IServiceCollection services, IConfiguration configuration)
        {
            if (!configuration.CanConfigureMongoDbStorage())
            {
                throw new InvalidOperationException("Can not configure MongoDb storage");
            }

            return services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)))
                .AddSingleton<IMongoClient, MongoClient>(provider => new MongoClient())
                .AddTransient<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(nameof(HC)));
        }
    }
}