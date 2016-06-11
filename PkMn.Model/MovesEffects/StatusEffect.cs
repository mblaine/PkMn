using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class StatusEffect : MoveEffect
    {
        public readonly StatusCondition Status;
        public readonly Who Who;
        public readonly int Chance;
        public readonly int TurnLimit;
        public readonly bool Reset;
        public readonly string ForceMessage;
        public readonly bool Force;
        public readonly string Condition;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "status", "who", "chance", "turn-limit", "force", "condition", "force-message" }).ToArray(); } }

        public StatusEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Status = (StatusCondition)Enum.Parse(typeof(StatusCondition), node.Attributes["status"].Value.Replace("-", ""), true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            Chance = node.Attributes.Contains("chance") ? int.Parse(node.Attributes["chance"].Value) : 256;
            TurnLimit = node.Attributes.Contains("turn-limit") ? int.Parse(node.Attributes["turn-limit"].Value) : 0;
            Reset = type == MoveEffectType.ResetStatus;
            ForceMessage = node.Attributes.Contains("force-message") ? node.Attributes["force-message"].Value : null;
            Force = node.Attributes.Contains("force") ? bool.Parse(node.Attributes["force"].Value) : false;
            Condition = node.Attributes.Contains("condition") ? node.Attributes["condition"].Value : null;
        }
    }
}
