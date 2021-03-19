using System;
using System.Collections.Generic;

namespace HC.Domain
{
    public class HttpCheck
    {
        private HttpCheck()
        {
        }

        public HttpCheck(Guid id, string uri, string? telegramChatId, TimeSpan timeout, ushort[] successStatusCodes,
            Dictionary<string, string> headers)
        {
            Headers = headers;
            Id = id;
            Uri = uri;
            Timeout = timeout;
            SuccessStatusCodes = successStatusCodes;
            TelegramChatId = telegramChatId;
        }

        public Guid Id { get;set; }
        public string Uri {
            get; set;
        }

        public TimeSpan Timeout { get; set; } 

        public IReadOnlyCollection<ushort> SuccessStatusCodes { get; set; }

        public string? TelegramChatId {
            get; set;
        }

        public Dictionary<string, string> Headers { get; set; } 
    }
}