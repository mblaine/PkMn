using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic
{
    public class Element
    {
        protected static string XmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Generation-I", "elements.xml");

        public static readonly Dictionary<String, Element> Elements = Load();

        public readonly string Name;
        public readonly ElementCategory Category;
        public readonly Dictionary<Element, decimal> Effectiveness;
        public readonly List<StatusCondition> Immunity;

        protected static Dictionary<String, Element> Load()
        {
            Dictionary<String, Element> t = new Dictionary<String, Element>();

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);

            foreach (XmlNode node in doc.GetElementsByTagName("element"))
            {
                Element type = new Element(node);
                t[type.Name] = type;
            }

            foreach (XmlNode node in doc.GetElementsByTagName("element"))
            {
                Element type = t[node.Attributes["name"].Value];
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "effectiveness")
                        type.Effectiveness[t[childNode.Attributes["type"].Value]] = decimal.Parse(childNode.Attributes["multiplier"].Value);
                    else if (childNode.Name == "immunity")
                        type.Immunity.Add((StatusCondition)Enum.Parse(typeof(StatusCondition), childNode.Attributes["status"].Value, true));
                }
            }

            return t;
        }

        protected Element(XmlNode node)
        {
            this.Name = node.Attributes["name"].Value;
            this.Category = (ElementCategory)Enum.Parse(typeof(ElementCategory), node.Attributes["category"].Value, true);
            this.Effectiveness = new Dictionary<Element, decimal>();
            this.Immunity = new List<StatusCondition>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
