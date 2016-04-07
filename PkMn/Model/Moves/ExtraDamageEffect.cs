using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class ExtraDamageEffect : MoveEffect
    {
        public readonly int Value;
        public readonly decimal Percent;
        public readonly string Message;

        protected override string[] ValidAttributes { get { return new string[] { "type", "value", "percent", "message" }; } }

        public ExtraDamageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Value = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "value") ? int.Parse(node.Attributes["value"].Value) : 0;
            Percent = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "percent") ? decimal.Parse(node.Attributes["percent"].Value) : 0m;
            Message = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "message") ? node.Attributes["message"].Value : null;
        }
    }
}
