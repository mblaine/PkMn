using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model
{
    public class Stats
    {
        public readonly string Type;
        public readonly int HP;
        public readonly int Attack;
        public readonly int Defense;
        public readonly int Special;
        public readonly int Speed;

        public Stats(XmlNode node)
        {
            Type = node.Attributes["type"].Value;
            HP = int.Parse(node.Attributes["hp"].Value);
            Attack = int.Parse(node.Attributes["attack"].Value);
            Defense = int.Parse(node.Attributes["defense"].Value);
            Special = int.Parse(node.Attributes["special"].Value);
            Speed = int.Parse(node.Attributes["speed"].Value);
        }
    }
}
