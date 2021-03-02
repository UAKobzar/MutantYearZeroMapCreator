using System.Xml.Serialization;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "nd")]
    public class Nd
    {
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
    }

}

