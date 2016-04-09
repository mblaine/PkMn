﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class ExtraDamageEffect : MoveEffect
    {
        public readonly int Value;
        public readonly decimal Percent;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "value", "percent" }).ToArray(); } }

        public ExtraDamageEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Value = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "value") ? int.Parse(node.Attributes["value"].Value) : 0;
            Percent = node.Attributes.Cast<XmlAttribute>().Any(a => a.Name == "percent") ? decimal.Parse(node.Attributes["percent"].Value) : 0m;
        }
    }
}
