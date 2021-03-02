using System.Xml.Serialization;
using System.Collections.Generic;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "way")]
    public class Way
    {
        [XmlElement(ElementName = "nd")]
        public List<Nd> Nd { get; set; }
        [XmlElement(ElementName = "tag")]
        public List<Tag> Tag { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

}

