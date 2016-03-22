using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic.Moves
{
    public class NoEffectEffect : MoveEffect
    {
        public readonly Element Condition;

        protected override string[] ValidAttributes { get { return new string[] { "type", "condition" }; } }

        public NoEffectEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Condition = Element.Elements[node.Attributes["condition"].Value];
        }
    }
}
