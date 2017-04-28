using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StormLambdaCommon
{
    [DataContract]
    public class DeviceEvent
    {
        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public int DeviceId { get; set; }

        [DataMember]
        public string DeviceCategory { get; set; }

        [DataMember]
        public string DeviceVersion { get; set; }

        [DataMember]
        public double Temparature { get; set; }

        public String ToXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceEvent));
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, this);
                return sw.ToString();
            }
        }

        public static DeviceEvent Parse(String xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceEvent));
            using (var sr = new StringReader(xml))
            {
                return (serializer.Deserialize(sr) as DeviceEvent);
            }
        }
    }
}
