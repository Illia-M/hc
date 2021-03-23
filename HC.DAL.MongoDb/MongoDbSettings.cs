using OptionsConfigurationServiceCollectionExtensions = Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions;

namespace HC.DAL.MongoDb
{
    public class MongoDbSettings
    {
        public string ConnectionString {
            get; set;
        }
    }
}
