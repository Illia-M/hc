using System;
using System.Threading.Tasks;
using HC.Adapters.Telegram;
using HC.ApplicationServices.Checks.HttpCheck;
using HC.ApplicationServices.History;
using HC.ApplicationServices.Notifications;
using HC.DAL.MongoDb;
using HC.Domain;
using HC.Http;
using HC.Settings;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;
using Telegram.Bot;
using HttpCheckSettingsLiteDBRepository = HC.DAL.LiteDB.HttpCheckSettingsLiteDBRepository;

namespace HC
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .UseSerilog((context, configuration) => {
                    configuration
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .WriteTo.Console();

                    if (context.Configuration.CanConfigureMongoDbStorage())
                    {
                        var mongoUrl = context.Configuration.GetSection(nameof(MongoDbSettings))?[nameof(MongoDbSettings.ConnectionString)];
                        configuration.WriteTo.MongoDB(mongoUrl);
                    }
                    else
                    {
                        configuration.WriteTo.File("app.log");
                    }
                })
                .ConfigureServices((hostContext, services) => {
                    services.AddOptions();
                    services.AddHttpClient();
                    services.Configure<AppSettings>(hostContext.Configuration);
                    services.Configure<TelegramSettings>(hostContext.Configuration.GetSection(nameof(AppSettings.Telegram)));
                    services.Configure<ChecksSettings>(hostContext.Configuration.GetSection(nameof(AppSettings.Checks)));
                    services.AddSingleton<INotificationService, TelegramNotificationService>();
                    services.AddSingleton<StatusWriter>();
                    services.AddTransient<HttpCheckExecutor>();

                    if (hostContext.Configuration.CanConfigureMongoDbStorage())
                    {
                        services.AddMongoDbDal(hostContext.Configuration);
                        services.AddTransient<IHttpCheckSettingsRepository, HttpCheckSettingsMongoDbRepository>();
                    }
                    else
                    {
                        services.AddSingleton(provider => new LiteDatabase("app.db"));
                        services.AddTransient<IHttpCheckSettingsRepository, HttpCheckSettingsLiteDBRepository>();
                    }

                    services.AddSingleton<TelegramBotClient>(provider => {
                        var options = provider.GetRequiredService<IOptions<TelegramSettings>>();

                        if (!string.IsNullOrEmpty(options.Value?.Token))
                        {
                            return new TelegramBotClient(options.Value.Token);
                        }

                        var logger = provider.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Telegram not configured");

                        return null!;
                    });

                    services.AddHostedService<TelegramHostedService>();
                    services.AddHostedService<HttpCheckWorker>();
                });
        }
    }
}
