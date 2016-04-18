using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model
{
    public class DexEntry
    {
        public readonly Species Species;

        public readonly string SpeciesDescription;
        public readonly string EntryText;
        public readonly DexColor Color;

        public readonly string HeightImperial;
        public readonly string HeightMetric;

        public readonly string WeightImperial;
        public readonly string WeightMetric;

        public DexEntry(Species species, XmlNode node)
        {
            Color = (DexColor)Enum.Parse(typeof(DexColor), node.Attributes["color"].Value, true);
            SpeciesDescription= node.Attributes["dex-species"].Value;
            EntryText = node.Attributes["dex-entry"].Value;

            XmlNode ht = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "measurement" && n.Attributes["type"].Value == "height").First();
            XmlNode wt = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "measurement" && n.Attributes["type"].Value == "weight").First();

            HeightImperial = ht.Attributes["imperial"].Value;
            HeightMetric = ht.Attributes["metric"].Value;

            WeightImperial = wt.Attributes["imperial"].Value;
            WeightMetric = wt.Attributes["metric"].Value;
        }
    }
}
