using System;

namespace HC.Domain
{
    public class ChecksSettings
    {
        public HttpCheckSettings[] HttpChecks { get; set; } = Array.Empty<HttpCheckSettings>();
    }
}