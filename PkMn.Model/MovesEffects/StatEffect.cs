using System;
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

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "stat", "who", "change", "multiplier", "chance", "temporary", "condition", "limit", "constant" }).ToArray(); } }

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
            Constant = node.Attributes.Contains("constant") ? int.Parse(node.Attributes["constant"].Value) : 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}% chance to ", Math.Round(((decimal)Chance) / 255m * 100m, 0));

            string whoStr = Who == Who.Self ? "own" : string.Format("{0}'s", Who.ToString().ToLower());

            string statStr = Stat == StatType.CritRatio ? "critical hit ratio" : Stat.ToString().ToLower();

            if (Constant != 0)
                sb.AppendFormat("set {0} {1} to {2}", whoStr, statStr, Constant);
            else if (Multiplier != 1m)
                sb.AppendFormat("multiply {0} current {1} by {2}", whoStr, statStr, Multiplier);
            else
                sb.AppendFormat("{0} {1} {2} stat stage by {3}", Change > 0 ? "raise" : "lower", whoStr, statStr, Math.Abs(Change));

            if (Temporary)
                sb.Append(" for the current turn only");

            if (Condition == "on-damaged")
                sb.AppendFormat(" on damage received");
            else if(!string.IsNullOrWhiteSpace(Condition))
                sb.AppendFormat(" on {0}", Condition.Replace('-', ' '));

            return sb.ToString();
        }
    }
}
