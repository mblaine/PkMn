using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class MultiEffect : MoveEffect
    {
        public readonly int Min;
        public readonly int Max;
        public readonly When When;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "min", "max", "when", "constant-damage", "ignore-cancel", "ignore-miss-on-lock" }).ToArray(); } }

        public MultiEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Min = node.Attributes.Contains("min") ? int.Parse(node.Attributes["min"].Value) : int.MaxValue - 1;
            Max = node.Attributes.Contains("max") ? int.Parse(node.Attributes["max"].Value) : int.MaxValue - 1;

            When = node.Attributes.Contains("when") ? (When)Enum.Parse(typeof(When), node.Attributes["when"].Value, true) : Enums.When.NA;
        }
    }
}
