using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic
{
    public class Evolution
    {
        public readonly string Name;
        public readonly EvolutionType Type;
        public readonly string Condition;

        public Species Species
        {
            get { return Species.Spp[Name]; }
        }

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
