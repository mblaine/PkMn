using System.Linq;
using System.Xml;
using PkMn.Model.Enums;
using System.Globalization;

namespace PkMn.Model.MoveEffects
{
    public class ExtraDamageEffect : MoveEffect
    {
        public readonly int Value;
        public readonly decimal Percent;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "value", "percent" }).ToArray(); } }

        public ExtraDamageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Value = node.Attributes.Contains("value") ? int.Parse(node.Attributes["value"].Value) : 0;
            Percent = node.Attributes.Contains("percent") ? decimal.Parse(node.Attributes["percent"].Value, CultureInfo.InvariantCulture) : 0m;
        }

        public override string ToString()
        {
            if (Type == MoveEffectType.RecoilDamage)
                return string.Format("Attacker receives {0}% of damage as recoil", Percent);
            else if (Type == MoveEffectType.MissDamage)
                return string.Format("Attacker receives {0} hp of damage if it misses", Value);
            else
                return base.ToString();
        }
    }
}
