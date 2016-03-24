using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class StatusEffect : MoveEffect
    {
        public readonly StatusCondition Status;
        public readonly Who Who;
        public readonly int Chance;
        public readonly int TurnLimit;
        public readonly bool Reset;

        protected override string[] ValidAttributes { get { return new string[] { "type", "status", "who", "chance", "turn-limit" }; } }

        public StatusEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Status = (StatusCondition)Enum.Parse(typeof(StatusCondition), node.Attributes["status"].Value.Replace("-", ""), true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            Chance = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "chance") ? int.Parse(node.Attributes["chance"].Value) : 256;
            TurnLimit = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "turn-limit") ? int.Parse(node.Attributes["turn-limit"].Value) : 256;
            Reset = type == MoveEffectType.ResetStatus;
        }
    }
}
