using System;
using HC.Adapters.Telegram;
using HC.Http;

namespace HC.Settings
{
    public class AppSettings
    {
        public HttpCheckSettings[] HttpChecks { get; set; } = Array.Empty<HttpCheckSettings>();

        public TelegramSettings? Telegram { get; set; }
    }
}