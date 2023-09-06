using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

using EventLogCapturer;

namespace UnitsTests
{
    [TestClass]
    public class ParsingXmlTests
    {
        [TestMethod]
        public void BasicParse()
        {
            var strData = File.ReadAllText("event.xml");
            var xmlData = new Log4jEvent(strData);
        }
    }
}
