using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class SubstituteEffect : MoveEffect
    {
        public readonly decimal HPPercent;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "hp-percent" }).ToArray(); } }

        public SubstituteEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            HPPercent = decimal.Parse(node.Attributes["hp-percent"].Value);
        }
    }
}
