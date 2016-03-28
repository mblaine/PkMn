using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;

namespace PkMn.Instance
{
    public class BattleStats : Stats
    {
        public int Evade;
        public int Accuracy;

        public BattleStats()
            : base()
        {
            Evade = 0;
            Accuracy = 0;
        }

        public override string ToString()
        {
            return string.Format("Atk:{0} Def:{1} Spc:{2} Spd:{3} Eva:{4} Acc:{5}", Attack, Defense, Special, Speed, Evade, Accuracy);
        }
    }
}
