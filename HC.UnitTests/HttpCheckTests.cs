using System;
using HC.Domain.HttpCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HC.UnitTests
{
    [TestClass]
    public class HttpCheckTests
    {
        [TestMethod]
        public void Given_HttpCheckParameters_When_CreateHttpCheck_Then_TimeoutSameAsTimeoutParameter()
        {
            var sut = new HttpCheck(
                id: Guid.NewGuid(),
                uri: "http://some.com",
                telegramChatId: null,
                timeout: TimeSpan.FromSeconds(30),
                successStatusCodes: new ushort[] { 200 },
                headers: new System.Collections.Generic.Dictionary<string, string>());

            Assert.IsNotNull(sut);
            Assert.IsTrue(sut.Timeout == TimeSpan.FromSeconds(30));
        }
    }
}
