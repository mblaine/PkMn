﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

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
            Multiplier = node.Attributes.Contains("multiplier") ? decimal.Parse(node.Attributes["multiplier"].Value) : 1m;
        }
    }
}
