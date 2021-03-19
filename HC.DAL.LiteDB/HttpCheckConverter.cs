using System;
using HC.Domain;

namespace HC.DAL.LiteDB
{
    public static class HttpCheckConverter
    {
        public static HttpCheck ToDomain(this HttpCheckSettings httpCheckSettings, Guid? id = null)
        {
            return new HttpCheck(id ?? Guid.NewGuid(), httpCheckSettings.Uri, httpCheckSettings.TelegramChatId,
                httpCheckSettings.Timeout, httpCheckSettings.SuccessStatusCodes, httpCheckSettings.Headers);
        }
    }
}