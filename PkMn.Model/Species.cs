using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model
{
    public class Species
    {
        protected static string XmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Generation-I", "species.xml");

        public static readonly Dictionary<string, Species> Spp = Load();

        public static readonly Dictionary<ExpGrowthRate, int[]> ExpRequired = Calculate();

        public readonly int Number;
        public readonly string Name;
        public readonly Element Type1;
        public readonly Element Type2;

        public readonly int CatchRate;
        public readonly int BaseExp;
        public readonly ExpGrowthRate ExpGrowthRate;

        public readonly List<Evolution> Evolutions;
        public readonly List<Learnset> Learnset;

        public readonly Stats BaseStats;

        public readonly BodyType BodyType;

        public readonly DexEntry DexEntry;

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

        protected static Dictionary<ExpGrowthRate, int[]> Calculate()
        {
            Dictionary<ExpGrowthRate, int[]> ret = new Dictionary<ExpGrowthRate, int[]>();

            ret[ExpGrowthRate.Slow] = new int[101];
            ret[ExpGrowthRate.Fast] = new int[101];
            ret[ExpGrowthRate.MediumSlow] = new int[101];
            ret[ExpGrowthRate.MediumFast] = new int[101];

            for (int i = 0; i <= 100; i++)
            {
                ret[ExpGrowthRate.Slow][i] = (int)(5m * i * i * i / 4m);
                ret[ExpGrowthRate.Fast][i] = (int)(4m * i * i * i / 5m);
                ret[ExpGrowthRate.MediumSlow][i] = (int)(6m/5m * i * i * i - 15m * i * i + 100m * i - 140m);
                ret[ExpGrowthRate.MediumFast][i] = i * i * i;
            }

            return ret;
        }

        protected Species(XmlNode node)
        {
            Number = int.Parse(node.Attributes["number"].Value);
            Name = node.Attributes["name"].Value;
            Type1 = Element.Elements[node.Attributes["type-1"].Value];
            
            if(node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "type-2"))
                Type2 = Element.Elements[node.Attributes["type-2"].Value];

            CatchRate = int.Parse(node.Attributes["catch-rate"].Value);
            BaseExp = int.Parse(node.Attributes["base-exp"].Value);
            ExpGrowthRate = (ExpGrowthRate)Enum.Parse(typeof(ExpGrowthRate), node.Attributes["exp-growth-rate"].Value.Replace("-", ""), true);

            DexEntry = new DexEntry(this, node);

            Evolutions = new List<Evolution>();
            foreach (var e in node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "evolution"))
            {
                Evolutions.Add(new Evolution(e));
            }

            Learnset = new List<Learnset>();
            foreach (var m in node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "moves").First().ChildNodes.Cast<XmlNode>())
            {
                Learnset.Add(new Learnset(m));
            }

            BaseStats = new Stats(node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "stats").First());

            BodyType = (BodyType)Enum.Parse(typeof(BodyType), node.Attributes["body-type"].Value, true);
        }

        public bool IsImmuneToStatus(StatusCondition status)
        {
            return Type1.Immunity.Contains(status) || (Type2 != null && Type2.Immunity.Contains(status));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}: {1} ({2}", Number, Name, Type1.Name);
            if (Type2 != null)
                sb.AppendFormat(" / {0})", Type2.Name);
            else
                sb.Append(")");

            return sb.ToString();
        }
    }
}
