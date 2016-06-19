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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Type == MoveEffectType.Status)
            {
                sb.AppendFormat("{0}% chance to ", Math.Round(((decimal)Chance) / 255m * 100m, 0));

                switch (Status)
                {
                    case StatusCondition.BadlyPoisoned:
                        sb.AppendFormat("badly poison {0}", Who.ToString().ToLower());
                        break;
                    case StatusCondition.Confusion:
                        sb.AppendFormat("confuse {0}", Who.ToString().ToLower());
                        break;
                    case StatusCondition.Paralysis:
                        sb.AppendFormat("paralyze {0}", Who.ToString().ToLower());
                        break;
                    case StatusCondition.Faint:
                    case StatusCondition.Flinch:
                        sb.AppendFormat("cause {0} to {1}", Who.ToString().ToLower(), Status.ToString().ToLower());
                        break;
                    case StatusCondition.Sleep:
                        sb.AppendFormat("{0} {1} to fall asleep", Force ? "force" : "cause", Who.ToString().ToLower());
                        break;
                    default:
                        sb.AppendFormat("{0} {1}", Status.ToString().ToLower(), Who.ToString().ToLower());
                        break;
                }

                if (!string.IsNullOrWhiteSpace(Condition))
                    sb.AppendFormat(" on {0}", Condition.Replace('-', ' '));
            }
            else if (Type == MoveEffectType.ResetStatus)
            {
                switch (Status)
                {
                    case StatusCondition.BadlyPoisoned:
                        sb.AppendFormat("Converts badly poisoned to just poisoned for {0}", Who.ToString().ToLower());
                        break;
                    case StatusCondition.Confusion:
                        sb.AppendFormat("Removes confusion from {0}", Who.ToString().ToLower());
                        break;
                    case StatusCondition.All:
                        sb.AppendFormat("Removes all status conditions from {0}", Who.ToString().ToLower());
                        break;
                    default:
                        sb.AppendFormat("Removes status {0} from {1}", Status.ToString().ToLower(), Who.ToString().ToLower());
                        break;
                }
            }
            else
                sb.Append(base.ToString());

            return sb.ToString();
        }
    }
}
