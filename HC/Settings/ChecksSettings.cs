using System;

namespace HC.Settings
{
    public class ChecksSettings
    {
        public HttpCheckSettings[] HttpChecks { get; set; } = Array.Empty<HttpCheckSettings>();
    }
}