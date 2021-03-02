using System.Xml.Serialization;

namespace MYZMC.Entities.OpenData
{
    [XmlRoot(ElementName = "meta")]
    public class Meta
    {
        [XmlAttribute(AttributeName = "osm_base")]
        public string Osm_base { get; set; }
    }

}

