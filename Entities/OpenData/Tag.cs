using System.Xml.Serialization;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "tag")]
    public class Tag
    {
        [XmlAttribute(AttributeName = "k")]
        public string K { get; set; }
        [XmlAttribute(AttributeName = "v")]
        public string V { get; set; }
    }

}

