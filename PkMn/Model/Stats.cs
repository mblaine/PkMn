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
        public int HP;
        public int Attack;
        public int Defense;
        public int Special;
        public int Speed;

        public Stats()
        {
            HP = 0;
            Attack = 0;
            Defense = 0;
            Special = 0;
            Speed = 0;
        }

        public Stats(XmlNode node)
        {
            HP = int.Parse(node.Attributes["hp"].Value);
            Attack = int.Parse(node.Attributes["attack"].Value);
            Defense = int.Parse(node.Attributes["defense"].Value);
            Special = int.Parse(node.Attributes["special"].Value);
            Speed = int.Parse(node.Attributes["speed"].Value);
        }

        public override string ToString()
        {
            return string.Format("HP:{0} Atk:{1} Def:{2} Spc:{3} Spd:{4}", HP, Attack, Defense, Special, Speed);
        }
    }
}
