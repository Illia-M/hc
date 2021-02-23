using System;
using System.Text.Json.Serialization;

namespace hc.History
{
    public class CheckStatus
    {
        private CheckStatus(){}

        public CheckStatus(string resource, bool isCheckPass, long elapsedMilliseconds, DateTimeOffset atTime)
        {
            AtTime = atTime;
            Resource = resource;
            IsCheckPass = isCheckPass;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        [JsonPropertyName("at_time")]
        public DateTimeOffset AtTime { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; } = null!;

        [JsonPropertyName("is_check_pass")]
        public bool IsCheckPass { get; set; }

        [JsonPropertyName("elapsed_milliseconds")]
        public long ElapsedMilliseconds { get; set; }
    }
}