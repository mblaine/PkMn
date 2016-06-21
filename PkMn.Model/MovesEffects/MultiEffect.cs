using System;
using System.Linq;
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

        public override string ToString()
        {
            switch (Type)
            {
                case MoveEffectType.Charge:
                    return string.Format("Must spend a turn {0}charging {1} dealing damage", When == When.After ? "re" : "", When.ToString().ToLower());
                case MoveEffectType.MultiHit:
                    if (Min == Max)
                        return string.Format("Attack hits {0} times", Max);
                    else
                        return string.Format("Attack hits between {0} and {1} times", Min, Max);
                case MoveEffectType.Disable:
                    return string.Format("Disables one of the foe's moves for {0} to {1} turns", Min, Max);
                default:
                    return base.ToString();
            }
        }
    }
}
