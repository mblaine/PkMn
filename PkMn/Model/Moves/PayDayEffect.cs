﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class PayDayEffect : MoveEffect
    {
        public readonly decimal Multiplier;

        protected override string[] ValidAttributes { get { return new string[] { "type", "multiplier" }; } }

        public PayDayEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Multiplier = decimal.Parse(node.Attributes["multiplier"].Value);
        }
    }
}