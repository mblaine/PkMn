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
    public class Species
    {
        protected static string XmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Generation-I", "species.xml");

        public static readonly Dictionary<string, Species> Spp = Load();

        public readonly int Number;
        public readonly string Name;
        public readonly Element Type1;
        public readonly Element Type2;
        public readonly string DexEntry;
        public readonly List<Evolution> Evolutions;

        protected Species(XmlNode node)
        {
            Number = int.Parse(node.Attributes["number"].Value);
            Name = node.Attributes["name"].Value;
            Type1 = Element.Elements[node.Attributes["type-1"].Value];
            
            if(node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "type-2"))
                Type2 = Element.Elements[node.Attributes["type-2"].Value];
            
            DexEntry= node.Attributes["dex-entry"].Value;

            Evolutions = new List<Evolution>();
            foreach (var e in node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "evolution"))
            {
                Evolutions.Add(new Evolution(e));
            }
        }

        protected static Dictionary<string, Species> Load()
        {
            Dictionary<string, Species> t = new Dictionary<string, Species>();

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);

            foreach (XmlNode node in doc.GetElementsByTagName("species"))
            {
                Species species = new Species(node);
                t[species.Name] = species;
            }

            return t;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}: {1} ({2}", Number, Name, Type1.Name);
            if (Type2 != null)
                sb.AppendFormat(" / {0})", Type2.Name);
            else
                sb.Append(")");

            sb.AppendFormat(" - {0}", DexEntry);

            return sb.ToString();
                

        }
    }

    public class Evolution
    {
        public readonly string Name;
        public readonly EvolutionType Type;
        public readonly string Condition;

        public Evolution(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            Type = (EvolutionType)Enum.Parse(typeof(EvolutionType), node.Attributes["type"].Value, true);
            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "condition"))
                Condition = node.Attributes["condition"].Value;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case EvolutionType.Level:
                    return string.Format("Evolves into {0} at level {1}", Name, Condition);
                case EvolutionType.Stone:
                    return string.Format("Evolves into {0} with a {1} Stone", Name, Condition);
                case EvolutionType.Trade:
                    return string.Format("Evolves into {0} when traded", Name, Condition);
                default:
                    throw new Exception();
            }
        }
    }
}
