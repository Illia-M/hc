using System;
using hc.Http;
using hc.Notify;

namespace hc.Settings
{
    public class AppSettings
    {
        public HttpCheckSettings[] HttpChecks { get; set; } = Array.Empty<HttpCheckSettings>();

        public TelegramSettings? Telegram { get; set; }
    }
}