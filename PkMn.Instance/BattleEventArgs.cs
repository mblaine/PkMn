using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model.Enums;

namespace PkMn.Instance
{
    public class BattleEventArgs : EventArgs
    {
        public readonly BattleEventType Type;
        public readonly ActiveMonster Monster;
        public readonly int HPBefore;
        public readonly int HPAfter;

        public BattleEventArgs(BattleEventType type, ActiveMonster monster, int hpBefore, int hpAfter)
        {
            Type = type;
            Monster = monster;
            HPBefore = hpBefore;
            HPAfter = hpAfter;
        }
    }
}
