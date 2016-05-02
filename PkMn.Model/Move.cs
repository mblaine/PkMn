using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;
using PkMn.Model.MoveEffects;

namespace PkMn.Model
{
    public class Move
    {
        protected static string XmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Generation-I", "moves.xml");

        public static readonly Dictionary<string, Move> Moves = Load();

        public readonly string Name;
        public readonly Element Type;
        public readonly int PP;
        public readonly int Power;
        public readonly int Accuracy;
        public readonly int Priority;
        public readonly int CritRatio;
        public readonly ElementCategory Category;
        public readonly AttackType AttackType;

        public readonly List<MoveEffect> Effects;

        protected static Dictionary<string, Move> Load()
        {
            Dictionary<string, Move> t = new Dictionary<string, Move>();

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);

            foreach (XmlNode node in doc.GetElementsByTagName("move"))
            {
                Move move = new Move(node);
                t[move.Name] = move;
            }

            return t;
        }

        protected Move(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            Type = Element.Elements[node.Attributes["type"].Value];
            PP = int.Parse(node.Attributes["pp"].Value);
            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "power"))
                Power = int.Parse(node.Attributes["power"].Value);
            else
                Power = 0;
            Accuracy = int.Parse(node.Attributes["accuracy"].Value);
            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "category"))
                Category = (ElementCategory)Enum.Parse(typeof(ElementCategory), node.Attributes["category"].Value, true);
            else
                Category = Type.Category;

            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "priority"))
                Priority = int.Parse(node.Attributes["priority"].Value);
            else
                Priority = 0;

            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "crit-ratio"))
                CritRatio = int.Parse(node.Attributes["crit-ratio"].Value);
            else
                CritRatio = 1;

            Effects = new List<MoveEffect>();
            foreach (XmlNode effect in node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "effect"))
            {
                MoveEffectType effectType = (MoveEffectType)Enum.Parse(typeof(MoveEffectType), effect.Attributes["type"].Value.Replace("-", ""), true);

                switch (effectType)
                {
                    case MoveEffectType.Status:
                    case MoveEffectType.ResetStatus:
                        Effects.Add(new StatusEffect(effectType, effect));
                        break;
                    case MoveEffectType.Stat:
                        Effects.Add(new StatEffect(effectType, effect));
                        break;
                    case MoveEffectType.MultiHit:
                    case MoveEffectType.Disable:
                    case MoveEffectType.Charge:
                        Effects.Add(new MultiEffect(effectType, effect));
                        break;
                    case MoveEffectType.LockInMove:
                        Effects.Add(new LockInEffect(effectType, effect));
                        break;
                    case MoveEffectType.CustomDamage:
                        Effects.Add(new CustomDamageEffect(effectType, effect));
                        break;
                    case MoveEffectType.Copy:
                        Effects.Add(new CopyEffect(effectType, effect));
                        break;
                    case MoveEffectType.TransferHealth:
                    case MoveEffectType.RestoreHealth:
                        Effects.Add(new HealthEffect(effectType, effect));
                        break;
                    case MoveEffectType.ResetStatStages:
                    case MoveEffectType.ProtectStatStages:
                        Effects.Add(new StatStageEffect(effectType, effect));
                        break;
                    case MoveEffectType.RecoilDamage:
                    case MoveEffectType.MissDamage:
                        Effects.Add(new ExtraDamageEffect(effectType, effect));
                        break;
                    case MoveEffectType.StatusRequirement:
                        Effects.Add(new StatusRequirementEffect(effectType, effect));
                        break;
                    case MoveEffectType.PayDay:
                        Effects.Add(new PayDayEffect(effectType, effect));
                        break;
                    case MoveEffectType.NoEffect:
                        Effects.Add(new NoEffectEffect(effectType, effect));
                        break;
                    default:
                        Effects.Add(new MoveEffect(effectType, effect));
                        break;
                }
            }

            if (Power > 0 || Effects.Any(e => e.Type == MoveEffectType.CustomDamage))
            {
                if (Effects.Count == 0)
                    AttackType = AttackType.Damaging;
                else
                    AttackType = AttackType.DamagingWithEffectChance;
            }
            else if (Power <= 0)
            {
                if (Effects.Count(e => e.Type == MoveEffectType.Status && ((StatusEffect)e).Who == Who.Foe && ((StatusEffect)e).Status != StatusCondition.Paralysis) == Effects.Count)
                    AttackType = AttackType.NonDamaging;
                else if (Effects.Count(e => e.Type == MoveEffectType.Stat && ((StatEffect)e).Who == Who.Foe) == Effects.Count)
                    AttackType = AttackType.NonDamaging;
                else
                    AttackType = AttackType.None;
            }
        }

        public bool CanCauseStatus(StatusCondition status, Who who)
        {
            StatusEffect[] effs = Effects.Where(e => e is StatusEffect).Cast<StatusEffect>().ToArray();
            return effs.Any(e => e.Status == status && (e.Who == who || e.Who == Who.Both));
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}) - PP: {2} Power: {3} Accuracy: {4}", Name, Type.Name, PP, Power, Accuracy);
        }
    }
}
