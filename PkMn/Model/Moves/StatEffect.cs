using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
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
        public readonly string Message;

        protected override string[] ValidAttributes { get { return new string[] { "type", "stat", "who", "change", "multiplier", "chance", "temporary", "condition", "limit", "ignore-if-critical", "message", "constant" }; } }

        public StatEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Stat = (StatType)Enum.Parse(typeof(StatType), node.Attributes["stat"].Value.Replace("-", ""), true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);

            string[] attrs = node.Attributes.Cast<XmlAttribute>().Select(a => a.Name).ToArray();

            Change = attrs.Contains("change") ? int.Parse(node.Attributes["change"].Value) : 0;
            Multiplier = attrs.Contains("multiplier") ? decimal.Parse(node.Attributes["multiplier"].Value) : 1m;

            Chance = attrs.Contains("chance") ? int.Parse(node.Attributes["chance"].Value) : 256;
            Temporary = attrs.Contains("temporary") ? bool.Parse(node.Attributes["temporary"].Value) : false;
            Condition = attrs.Contains("condition") ? node.Attributes["condition"].Value : null;
            Limit = attrs.Contains("limit") ? int.Parse(node.Attributes["limit"].Value) : int.MaxValue;
            IgnoreIfCritical = attrs.Contains("ignore-if-critical") ? bool.Parse(node.Attributes["ignore-if-critical"].Value) : false;
            Message = attrs.Contains("message") ? node.Attributes["message"].Value : "";
            Constant = attrs.Contains("constant") ? int.Parse(node.Attributes["constant"].Value) : 0;
        }
    }
}
