using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class StatEffect : MoveEffect
    {
        public readonly StatType Stat;
        public readonly Who Who;

        public readonly int Change;
        public readonly decimal Multiplier;
        public readonly int Constant;

        public readonly int Chance;
        public readonly bool Temporary;
        public readonly string Condition;
        public readonly int Limit;
        public readonly bool IgnoreIfCritical;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "stat", "who", "change", "multiplier", "chance", "temporary", "condition", "limit", "ignore-if-critical", "constant" }).ToArray(); } }

        public StatEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Stat = (StatType)Enum.Parse(typeof(StatType), node.Attributes["stat"].Value.Replace("-", ""), true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);

            Change = node.Attributes.Contains("change") ? int.Parse(node.Attributes["change"].Value) : 0;
            Multiplier = node.Attributes.Contains("multiplier") ? decimal.Parse(node.Attributes["multiplier"].Value) : 1m;

            Chance = node.Attributes.Contains("chance") ? int.Parse(node.Attributes["chance"].Value) : 256;
            Temporary = node.Attributes.Contains("temporary") ? bool.Parse(node.Attributes["temporary"].Value) : false;
            Condition = node.Attributes.Contains("condition") ? node.Attributes["condition"].Value : null;
            Limit = node.Attributes.Contains("limit") ? int.Parse(node.Attributes["limit"].Value) : int.MaxValue;
            IgnoreIfCritical = node.Attributes.Contains("ignore-if-critical") ? bool.Parse(node.Attributes["ignore-if-critical"].Value) : false;
            Constant = node.Attributes.Contains("constant") ? int.Parse(node.Attributes["constant"].Value) : 0;
        }
    }
}
