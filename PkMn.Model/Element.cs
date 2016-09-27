using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using PkMn.Model.Enums;
using System.Globalization;

namespace PkMn.Model
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
                        type.Effectiveness[t[childNode.Attributes["type"].Value]] = decimal.Parse(childNode.Attributes["multiplier"].Value, CultureInfo.InvariantCulture);
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

        public decimal GetEffectiveness(Element e1, Element e2 = null)
        {
            decimal multiplier1 = 1m;
            decimal multiplier2 = 1m;
            if (this.Effectiveness.ContainsKey(e1))
                multiplier1 = this.Effectiveness[e1];
            if (e2 != null && this.Effectiveness.ContainsKey(e2))
                multiplier2 = this.Effectiveness[e2];
            return multiplier1 * multiplier2;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
