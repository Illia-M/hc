using System;
using System.Collections.Generic;

namespace HC.Domain.HttpCheck
{
    public class HttpCheck
    {
        public HttpCheck()
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

        public Guid Id { get; private set; }
        public string Uri {
            get; private set;
        }

        public TimeSpan Timeout { get; private set; } 

        public IReadOnlyCollection<ushort> SuccessStatusCodes { get; private set; }

        public string? TelegramChatId {
            get; private set;
        }

        public Dictionary<string, string> Headers { get; private set; } 
    }
}