using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Kip.Utils.Core
{
    public static class XmlUtils
    {
        public static T XmlDeserialize<T>(string responseXml)
        {
            return XmlDeserialize<T>(Encoding.UTF8.GetBytes(responseXml));
        }

        public static T XmlDeserialize<T>(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return XmlDeserialize<T>(stream);
        }

        public static T XmlDeserialize<T>(Stream inputStream)
        {
            var xs = new XmlSerializer(typeof(T), "");
            return (T)xs.Deserialize(inputStream);
        }

        private static object loopObjectSetCData(object obj)
        {
            if (null == obj) return obj;

            // 字符串值转换成CDATA
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propertyVal = property.GetValue(obj, null);
                if (property.PropertyType == typeof(string))
                {
                    if (null == propertyVal) propertyVal = "";

                    property.SetValue(obj, string.Format("[cdata_left]{0}[cdata_right]", propertyVal), null);
                }
                else if (typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    for (var i = 0; i < (propertyVal as IList).Count; i++)
                    {
                        (propertyVal as IList)[i] = loopObjectSetCData((propertyVal as IList)[i]);
                    }
                }
            }
            return obj;
        }

        public static string SerializeObjectToXml<T>(this T obj)
        {
            // 字符串值转换成CDATA
            loopObjectSetCData(obj);

            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                xmlSerializer.Serialize(textWriter, obj, ns);

                string xmlString = textWriter.ToString();
                return xmlString.Replace("[cdata_left]", "<![CDATA[").Replace("[cdata_right]", "]]>");
            }
        }
    }
}
