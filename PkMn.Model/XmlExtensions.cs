using System.Linq;
using System.Xml;

namespace PkMn.Model
{
    public static class XmlExtensions
    {
        public static void AppendAttribute(this XmlNode node, string key, object value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(key);
            attr.Value = value.ToString();
            node.Attributes.Append(attr);
        }

        public static bool Contains(this XmlAttributeCollection attrs, string name)
        {
            return attrs.Cast<XmlAttribute>().Any(a => a.Name == name);
        }
    }
}
