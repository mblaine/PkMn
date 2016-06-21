using System;
using System.Linq;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class StatStageEffect : MoveEffect
    {
        public readonly Who Who;
        public readonly bool Protect;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "who" }).ToArray(); } }

        public StatStageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            Protect = type == MoveEffectType.ProtectStatStages;
        }

        public override string ToString()
        {
            if (Type == MoveEffectType.ProtectStatStages)
                return string.Format("Protects against lowering of {0}'s stat stages", Who.ToString().ToLower());
            else if (Type == MoveEffectType.ResetStatStages)
                return string.Format("Resets stat stages of {0}", Who.ToString().ToLower());
            else
                return base.ToString();
        }
    }
}
