using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
