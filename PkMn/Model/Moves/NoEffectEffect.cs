using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class NoEffectEffect : MoveEffect
    {
        public readonly Element Condition;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "condition" }).ToArray(); } }

        public NoEffectEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Condition = Element.Elements[node.Attributes["condition"].Value];
        }
    }
}
