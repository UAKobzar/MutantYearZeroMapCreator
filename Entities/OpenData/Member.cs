using System.Xml.Serialization;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "member")]
    public class Member
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        [XmlAttribute(AttributeName = "role")]
        public string Role { get; set; }
    }

}

