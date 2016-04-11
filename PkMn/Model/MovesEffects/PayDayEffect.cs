using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class PayDayEffect : MoveEffect
    {
        public readonly decimal Multiplier;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "multiplier" }).ToArray(); } }

        public PayDayEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Multiplier = decimal.Parse(node.Attributes["multiplier"].Value);
        }
    }
}
