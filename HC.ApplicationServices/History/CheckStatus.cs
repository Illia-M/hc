using System;
using System.Text.Json.Serialization;

namespace HC.ApplicationServices.History
{
    public class CheckStatus
    {
        private CheckStatus() { }

        public CheckStatus(string resource, bool isCheckPass, long elapsedMilliseconds, DateTimeOffset atTime)
        {
            AtTime = atTime;
            Resource = resource;
            IsCheckPass = isCheckPass;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        [JsonPropertyName("at_time")]
        public DateTimeOffset AtTime { get; private set; }

        [JsonPropertyName("resource")]
        public string Resource { get; private set; } = null!;

        [JsonPropertyName("is_check_pass")]
        public bool IsCheckPass { get; private set; }

        [JsonPropertyName("elapsed_milliseconds")]
        public long ElapsedMilliseconds { get; private set; }
    }
}