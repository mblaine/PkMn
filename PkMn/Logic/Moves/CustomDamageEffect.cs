using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic.Moves
{
    public class CustomDamageEffect : MoveEffect
    {
        public readonly string Calculation;
        public readonly int Value;
        public readonly decimal Multiplier;

        protected override string[] ValidAttributes { get { return new string[] { "type", "calculation", "value", "multiplier" }; } }

        public CustomDamageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Calculation = node.Attributes["calculation"].Value;
            string[] attrs = node.Attributes.Cast<XmlAttribute>().Select(a => a.Name).ToArray();

            Value = attrs.Contains("value") ? int.Parse(node.Attributes["value"].Value) : 0;
            Multiplier = attrs.Contains("multiplier") ? decimal.Parse(node.Attributes["multiplier"].Value) : 1m;
        }
    }
}
