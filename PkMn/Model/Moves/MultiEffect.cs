using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class MultiEffect : MoveEffect
    {
        public readonly int Min;
        public readonly int Max;
        public readonly When When;
        public readonly string Message;

        protected override string[] ValidAttributes { get { return new string[] { "type", "min", "max", "when", "message", "constant-damage", "ignore-cancel", "ignore-miss-on-lock" }; } }

        public MultiEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            string[] attrs = node.Attributes.Cast<XmlAttribute>().Select(a => a.Name).ToArray();

            Min = attrs.Contains("min") ? int.Parse(node.Attributes["min"].Value) : int.MaxValue - 1;
            Max = attrs.Contains("max") ? int.Parse(node.Attributes["max"].Value) : int.MaxValue - 1;

            When = attrs.Contains("when") ? (When)Enum.Parse(typeof(When), node.Attributes["when"].Value, true) : Enums.When.NA;

            Message = attrs.Contains("message") ? node.Attributes["message"].Value : null;
        }
    }
}
