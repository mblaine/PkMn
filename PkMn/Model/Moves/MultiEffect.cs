﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class MultiEffect : MoveEffect
    {
        public readonly int Min;
        public readonly int Max;
        public readonly When When;
        public readonly string Message;

        protected override string[] ValidAttributes { get { return new string[] { "type", "min", "max", "when", "message" }; } }

        public MultiEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Min = int.Parse(node.Attributes["min"].Value);
            Max = int.Parse(node.Attributes["max"].Value);

            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "when"))
                When = (When)Enum.Parse(typeof(When), node.Attributes["when"].Value, true);

            if (node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "message"))
                Message = node.Attributes["message"].Value;
        }
    }
}
