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
        public static StatType[] StatTypes = new StatType[] { StatType.HP, StatType.Attack, StatType.Defense, StatType.Special, StatType.Speed };

        public int HP;
        public int Attack;
        public int Defense;
        public int Special;
        public int Speed;

        public int this[StatType s]
        {
            get
            {
                switch (s)
                {
                    case StatType.HP:
                        return HP;
                    case StatType.Attack:
                        return Attack;
                    case StatType.Defense:
                        return Defense;
                    case StatType.Speed:
                        return Speed;
                    case StatType.Special:
                        return Special;
                    default:
                        throw new Exception();
                }
            }

            set
            {
                switch (s)
                {
                    case StatType.HP:
                        HP = value;
                        break;
                    case StatType.Attack:
                        Attack = value;
                        break;
                    case StatType.Defense:
                        Defense = value;
                        break;
                    case StatType.Speed:
                        Speed = value;
                        break;
                    case StatType.Special:
                        Special = value;
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public Stats()
        {
            HP = 0;
            Attack = 0;
            Defense = 0;
            Special = 0;
            Speed = 0;
        }

        public Stats(Stats s)
        {
            HP = s.HP;
            Attack = s.Attack;
            Defense = s.Defense;
            Special = s.Special;
            Speed = s.Speed;
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
            return string.Format("HP:{0} Atk:{1} Def:{2} Spc:{3} Spe:{4}", HP, Attack, Defense, Special, Speed);
        }
    }
}
