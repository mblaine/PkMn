using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PkMn.Instance
{
    public class BattleEventArgs : EventArgs
    {
        public readonly string Message;
        public readonly ActiveMonster DamageDoneTo;
        public readonly int HPBefore;
        public readonly int HPAfter;

        public BattleEventArgs(string message, ActiveMonster damageDoneTo, int hpBefore, int hpAfter)
        {
            Message = message;
            DamageDoneTo = damageDoneTo;
            HPBefore = hpBefore;
            HPAfter = hpAfter;
        }
    }
}
