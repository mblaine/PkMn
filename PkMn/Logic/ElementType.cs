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
    public class ElementType
    {
        protected static string XmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Generation-I", "types.xml");

        public static readonly Dictionary<String, ElementType> Types = Load();

        public readonly string Name;
        public readonly TypeCategory Category;
        public readonly Dictionary<ElementType, decimal> Effectiveness;
        public readonly List<StatusCondition> Immunity;

        protected static Dictionary<String, ElementType> Load()
        {
            Dictionary<String, ElementType> t = new Dictionary<String, ElementType>();

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);

            foreach (XmlNode node in doc.GetElementsByTagName("type"))
            {
                ElementType type = new ElementType(node.Attributes["name"].Value, (TypeCategory)Enum.Parse(typeof(TypeCategory), node.Attributes["category"].Value, true));
                t[type.Name] = type;
            }

            foreach (XmlNode node in doc.GetElementsByTagName("type"))
            {
                ElementType type = t[node.Attributes["name"].Value];
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

        protected ElementType(string name, TypeCategory category)
        {
            this.Name = name;
            this.Category = category;
            this.Effectiveness = new Dictionary<ElementType, decimal>();
            this.Immunity = new List<StatusCondition>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
