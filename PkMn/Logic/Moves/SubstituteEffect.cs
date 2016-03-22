using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic.Moves
{
    public class SubstituteEffect : MoveEffect
    {
        public readonly decimal HPPercent;

        protected override string[] ValidAttributes { get { return new string[] { "type", "hp-percent" }; } }

        public SubstituteEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            HPPercent = decimal.Parse(node.Attributes["hp-percent"].Value);
        }
    }
}
