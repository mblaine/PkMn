using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class MoveEffect
    {
        public readonly MoveEffectType Type;
        public readonly string Message;

        protected virtual string[] ValidAttributes { get { return new string[] { "type", "message" }; } }

        public MoveEffect(MoveEffectType type, XmlNode node)
        {
            Type = type;
            if (node.Attributes.Contains("message"))
                Message = node.Attributes["message"].Value;
            else
                Message = null;
            Validate(node);
        }

        private void Validate(XmlNode node)
        {
            foreach (XmlAttribute attr in node.Attributes)
                if (!ValidAttributes.Contains(attr.Name))
                    throw new Exception(string.Format("Unrecognized attribute \"{0}\" in tag {1}", attr.Name, node.OuterXml));
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MoveEffectType.CancelEnemyMove:
                    return "Cancels foe's move each turn";
                case MoveEffectType.EndWildBattle:
                    return "Ends the battle if this is a wild battle";
                case MoveEffectType.IgnoreSemiInvulnerability:
                    return "Ignores semi-invulnerability when determining hit or miss";
                case MoveEffectType.IgnoreTypeEffectiveness:
                    return "Ignores type effectiveness when calculating damage";
                case MoveEffectType.IgnoreTypeImmunity:
                    return "Ignores type immunity when calculating damage";
                case MoveEffectType.LeechSeed:
                    return "Seeds foe such that 1/16th of its total hp is transferred after every turn";
                case MoveEffectType.MirrorMove:
                    return "Executes the same move as the foe's last turn";
                case MoveEffectType.MustBeFaster:
                    return "Attacker must have a greater effective speed stat or this will miss";
                case MoveEffectType.NeverDeductPP:
                    return "Never deduct PP when using this move";
                case MoveEffectType.None:
                    return "Does absolutely nothing";
                case MoveEffectType.OneHitKO:
                    return "If this move hits then the foe will faint";
                case MoveEffectType.PerfectAccuracy:
                    return "Skips accuracy and evasion check and assumes move will hit";
                case MoveEffectType.Random:
                    return "Executes a random move other than Struggle or Metronome";
                case MoveEffectType.SemiInvulnerable:
                    return "Attacker is semi-invulnerable while executing this move and most attacks will miss it";
                case MoveEffectType.Substitute:
                    return "Takes 25% of attacker's hp and creates a substitute which will absorb damage until it breaks";
                default:
                    return base.ToString();
            }
        }

    }
}
