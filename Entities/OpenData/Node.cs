using System.Xml.Serialization;
using System.Collections.Generic;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "node")]
    public class Node
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "lat")]
        public double Lat { get; set; }
        [XmlAttribute(AttributeName = "lon")]
        public double Lon { get; set; }
        [XmlElement(ElementName = "tag")]
        public List<Tag> Tag { get; set; }
    }

}

