using System;
using System.Linq;
using System.Xml;
using PkMn.Model.Enums;
using System.Globalization;

namespace PkMn.Model.MoveEffects
{
    public class HealthEffect : MoveEffect
    {
        public readonly decimal Percent;
        public readonly string Of;
        public readonly Who Who;
        public readonly bool RestoreOnly;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "percent", "of", "who" }).ToArray(); } }

        public HealthEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Percent = decimal.Parse(node.Attributes["percent"].Value, CultureInfo.InvariantCulture);
            Of = node.Attributes["of"].Value;
            if (node.Attributes.Contains("who"))
                Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            RestoreOnly = type == MoveEffectType.RestoreHealth;
        }

        public override string ToString()
        {
            if (Type == MoveEffectType.TransferHealth)
                return string.Format("Transfers {0}% of damage as health", Percent);
            else if (Type == MoveEffectType.RestoreHealth)
                return string.Format("Restores {0}% of max hp", Percent);
            else
                return base.ToString();
        }
    }
}
