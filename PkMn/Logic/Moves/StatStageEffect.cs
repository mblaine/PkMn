using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic.Moves
{
    public class StatStageEffect : MoveEffect
    {
        public readonly Who Who;
        public readonly bool Protect;

        protected override string[] ValidAttributes { get { return new string[] { "type", "who" }; } }

        public StatStageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            Protect = type == MoveEffectType.ProtectStatStages;
        }
    }
}
