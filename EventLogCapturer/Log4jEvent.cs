using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EventLogCapturer
{
    public class Log4jEvent
    {
        public string Logger { get; set; } = "NONE";
        public string Level { get; set; } = "NONE";
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;
        public string Message { get; set; } = "NONE";
        public Log4jEvent(string data)
        {
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(SanitizeData(data));
            var eventNode = xmldoc.DocumentElement.SelectSingleNode("/event");
            var msgNode = xmldoc.DocumentElement.SelectSingleNode("/event/message");

            Logger = eventNode.Attributes["logger"]?.InnerText;
            Level = eventNode.Attributes["level"]?.InnerText;
            var ticks = long.Parse(eventNode.Attributes["timestamp"]?.InnerText);
            TimeStamp = ConvertFromUnixTimestamp(ticks);
            Message = msgNode?.InnerText;
        }

        string SanitizeData(string xmldata)
        {
            return xmldata.Replace("log4j:", "");
        }
        DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(timestamp);
        }
    }
}
