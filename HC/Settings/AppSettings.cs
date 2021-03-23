using HC.Adapters.Telegram;

namespace HC.Settings
{
    public class AppSettings
    {
        public ChecksSettings Checks { get; set; }

        public TelegramSettings? Telegram { get; set; }
    }
}