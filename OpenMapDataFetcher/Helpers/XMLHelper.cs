using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MYZMC.OpenMapDataFetcher.Helpers
{
    class XMLHelper
    {
        public static T ParseXML<T>(string xml)
        {
            T returnObject = default(T);

            try
            {
                using (TextReader xmlStream = new StringReader(xml))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    returnObject = (T)serializer.Deserialize(xmlStream);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return returnObject;
        }
    }
}
