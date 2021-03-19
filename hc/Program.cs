using System.Threading.Tasks;
using HC.Adapters.Telegram;
using HC.ApplicationServices.History;
using HC.ApplicationServices.Notifications;
using HC.Http;
using HC.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;
using Telegram.Bot;

namespace HC
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
                    services.Configure<TelegramSettings>(hostContext.Configuration.GetSection(nameof(AppSettings.Telegram)));
                    services.AddSingleton<TelegramBotClient>(provider =>
                    {
                        var options = provider.GetRequiredService<IOptions<TelegramSettings>>();

                        if (!string.IsNullOrEmpty(options.Value?.Token))
                        {
                            return new TelegramBotClient(options.Value.Token);

                        }

                        var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();
                        logger.LogInformation("Telegram not configured");
                        return null;
                    });
                    services.AddSingleton<INotificationService, TelegramNotificationService>();
                    services.AddSingleton<StatusWriter>();
                    services.AddHostedService<TelegramHostedService>();
                    services.AddHostedService<HttpCheckWorker>();
                });
    }
}
