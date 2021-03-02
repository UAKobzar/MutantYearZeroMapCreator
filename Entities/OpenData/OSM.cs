using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace MYZMC.Entities.OpenData
{

    [XmlRoot(ElementName = "osm")]
    public class Osm
    {
        [XmlElement(ElementName = "note")]
        public string Note { get; set; }
        [XmlElement(ElementName = "meta")]
        public Meta Meta { get; set; }
        [XmlElement(ElementName = "node")]
        public List<Node> Node { get; set; }
        [XmlElement(ElementName = "way")]
        public List<Way> Way { get; set; }
        [XmlElement(ElementName = "relation")]
        public List<Relation> Relation { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "generator")]
        public string Generator { get; set; }
    }

}

