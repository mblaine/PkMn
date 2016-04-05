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
        public readonly string Message;
        public readonly bool Force;

        protected override string[] ValidAttributes { get { return new string[] { "type", "status", "who", "chance", "turn-limit", "message", "force" }; } }

        public StatusEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Status = (StatusCondition)Enum.Parse(typeof(StatusCondition), node.Attributes["status"].Value.Replace("-", ""), true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            Chance = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "chance") ? int.Parse(node.Attributes["chance"].Value) : 256;
            TurnLimit = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "turn-limit") ? int.Parse(node.Attributes["turn-limit"].Value) : 0;
            Reset = type == MoveEffectType.ResetStatus;
            Message = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "message") ? node.Attributes["message"].Value : null;
            Force = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "force") ? bool.Parse(node.Attributes["force"].Value) : false;
        }
    }
}
