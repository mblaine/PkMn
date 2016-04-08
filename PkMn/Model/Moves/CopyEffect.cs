using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class CopyEffect : MoveEffect
    {
        public readonly string What;
        public readonly int PP;
        public readonly string Message;

        protected override string[] ValidAttributes { get { return new string[] { "type", "what", "pp", "message" }; } }

        public CopyEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            What = node.Attributes["what"].Value;
            PP = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "pp") ? int.Parse(node.Attributes["pp"].Value) : 0;
            Message = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "message") ? node.Attributes["message"].Value : null;
        }
    }
}
