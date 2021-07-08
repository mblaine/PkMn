using System.Linq;
using System.Xml;
using PkMn.Model.Enums;
using System.Globalization;

namespace PkMn.Model.MoveEffects
{
    public class CustomDamageEffect : MoveEffect
    {
        public readonly string Calculation;
        public readonly int Value;
        public readonly decimal Multiplier;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "calculation", "value", "multiplier" }).ToArray(); } }

        public CustomDamageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Calculation = node.Attributes["calculation"].Value;

            Value = node.Attributes.Contains("value") ? int.Parse(node.Attributes["value"].Value) : 0;
            Multiplier = node.Attributes.Contains("multiplier") ? decimal.Parse(node.Attributes["multiplier"].Value, CultureInfo.InvariantCulture) : 1m;
        }

        public override string ToString()
        {
            if (Calculation == "level")
                return "Does damage equal to the attacker's level";
            else if (Calculation == "constant")
                return string.Format("Does a constant {0} hp of damage", Value);
            else if (Calculation == "foe-hp-remaining")
                return string.Format("Does damage equal to {0} x the foe's remaining hp", Multiplier);
            else if (Calculation == "rng-min-1-max-1.5x-level")
                return "Does a random amount of damage between 1 and 1.5 x the attacker's level";
            else if (Calculation == "last-damage-if-normal-or-fighting")
                return string.Format("Does damage equal to {0} x the last damage dealt by either side if the attack was normal or fighting and no damage otherwise", Multiplier);
            else if (Calculation == "accumulated-on-lock-end")
                return string.Format("Does damage on the turn lock in ends equal to {0} x the amount of damage received during the lock in", Multiplier);
            else
                return string.Format("Custom damage calculation: {0}", Calculation);

        }
    }
}
