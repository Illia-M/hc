using System.Threading.Tasks;
using hc.History;
using hc.Http;
using hc.Notify;
using hc.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace hc
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .WriteTo.Console()
                        .WriteTo.File("app.log");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<AppSettings>(hostContext.Configuration);
                    services.AddSingleton<INotificationService, TelegramService>();
                    services.AddSingleton<StatusWriter>();
                    services.AddHostedService<HttpCheckWorker>();
                });
    }
}
