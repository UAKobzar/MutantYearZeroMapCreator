using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "relation")]
    public class Relation
    {
        [XmlElement(ElementName = "member")]
        public List<Member> Member { get; set; }
        [XmlElement(ElementName = "tag")]
        public List<Tag> Tag { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }
}
