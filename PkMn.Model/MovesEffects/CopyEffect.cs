using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class CopyEffect : MoveEffect
    {
        public readonly string What;
        public readonly int PP;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "what", "pp" }).ToArray(); } }

        public CopyEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            What = node.Attributes["what"].Value;
            PP = node.Attributes.Contains("pp") ? int.Parse(node.Attributes["pp"].Value) : 0;
        }

        public override string ToString()
        {
            if (What == "all-moves")
                return "Copies all of the foe's moves";
            else if (What == "move")
                return "Copies ones of the foe's moves";
            else
                return string.Format("Copies the foe's {0}", What.Replace('-', ' '));
        }
    }
}
