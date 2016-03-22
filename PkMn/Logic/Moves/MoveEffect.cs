using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic.Moves
{
    public class MoveEffect
    {
        public readonly MoveEffectType Type;

        protected virtual string[] ValidAttributes { get { return new string[] { "type" }; } }

        public MoveEffect(MoveEffectType type, XmlNode node)
        {
            Type = type;
            Validate(node);
        }

        private void Validate(XmlNode node)
        {
            foreach (XmlAttribute attr in node.Attributes)
                if (!ValidAttributes.Contains(attr.Name))
                    throw new Exception(string.Format("Unrecognized attribute \"{0}\" in tag {1}", attr.Name, node.OuterXml));
        }

    }
}
