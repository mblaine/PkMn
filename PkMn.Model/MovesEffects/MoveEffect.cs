using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class MoveEffect
    {
        public readonly MoveEffectType Type;
        public readonly string Message;

        protected virtual string[] ValidAttributes { get { return new string[] { "type", "message" }; } }

        public MoveEffect(MoveEffectType type, XmlNode node)
        {
            Type = type;
            if (node.Attributes.Contains("message"))
                Message = node.Attributes["message"].Value;
            else
                Message = null;
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
