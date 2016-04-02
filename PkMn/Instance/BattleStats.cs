using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;
using PkMn.Model.Enums;

namespace PkMn.Instance
{
    public class BattleStats : Stats
    {
        public int Evade;
        public int Accuracy;
        public int CritRatio;

        public BattleStats()
            : base()
        {
            Evade = 0;
            Accuracy = 0;
            CritRatio = 0;
        }

        public new int this[StatType s]
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
                    case StatType.Evade:
                        return Evade;
                    case StatType.Accuracy:
                        return Accuracy;
                    case StatType.CritRatio:
                        return CritRatio;
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
                    case StatType.Evade:
                        Evade = value;
                        break;
                    case StatType.Accuracy:
                        Accuracy = value;
                        break;
                    case StatType.CritRatio:
                        CritRatio = value;
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Atk:{0} Def:{1} Spc:{2} Spd:{3} Eva:{4} Acc:{5}", Attack, Defense, Special, Speed, Evade, Accuracy);
        }
    }
}
