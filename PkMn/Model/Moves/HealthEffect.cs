using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class HealthEffect : MoveEffect
    {
        public readonly decimal Percent;
        public readonly string Of;
        public readonly Who Who;
        public readonly bool RestoreOnly;

        protected override string[] ValidAttributes { get { return new string[] { "type", "percent", "of", "who" }; } }

        public HealthEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Percent = decimal.Parse(node.Attributes["percent"].Value);
            Of = node.Attributes["of"].Value;
            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "who"))
                Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
            RestoreOnly = type == MoveEffectType.RestoreHealth;
        }
    }
}
