using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HC.DAL.MongoDb
{
    public static class MongoDbConfigurationExt
    {
        public static bool CanConfigureMongoDbStorage(this IConfiguration configuration)
        {
            var connectionString = configuration.GetSection(nameof(MongoDbSettings))?[nameof(MongoDbSettings.ConnectionString)];
            return !string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrEmpty(MongoUrl.Create(connectionString).DatabaseName);
        }

        public static IServiceCollection AddMongoDbDal(this IServiceCollection services, IConfiguration configuration)
        {
            if (!configuration.CanConfigureMongoDbStorage())
            {
                throw new InvalidOperationException("Can not configure MongoDb storage");
            }

            return services
                .Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)))
                .AddSingleton<MongoUrl>(provider => MongoUrl.Create(provider.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString))
                .AddSingleton<IMongoClient, MongoClient>(provider => new MongoClient(provider.GetRequiredService<MongoUrl>()))
                .AddTransient<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(nameof(HC)));
        }
    }
}