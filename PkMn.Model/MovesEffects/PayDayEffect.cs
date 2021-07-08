using System.Linq;
using System.Xml;
using PkMn.Model.Enums;
using System.Globalization;

namespace PkMn.Model.MoveEffects
{
    public class PayDayEffect : MoveEffect
    {
        public readonly decimal Multiplier;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "multiplier" }).ToArray(); } }

        public PayDayEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Multiplier = decimal.Parse(node.Attributes["multiplier"].Value, CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return string.Format("Causes {0} x the attacker's level to drop as money to be picked up by the winner of the battle", Multiplier);
        }
    }
}
