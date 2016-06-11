using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class ExtraDamageEffect : MoveEffect
    {
        public readonly int Value;
        public readonly decimal Percent;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "value", "percent" }).ToArray(); } }

        public ExtraDamageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Value = node.Attributes.Contains("value") ? int.Parse(node.Attributes["value"].Value) : 0;
            Percent = node.Attributes.Contains("percent") ? decimal.Parse(node.Attributes["percent"].Value) : 0m;
        }
    }
}
