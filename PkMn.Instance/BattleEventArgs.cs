using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model.Enums;
using PkMn.Model;

namespace PkMn.Instance
{
    public class BattleEventArgs : EventArgs
    {
        public readonly BattleEventType Type;
        public readonly ActiveMonster Monster;
        public readonly int HPBefore;
        public readonly int HPAfter;
        public readonly Move Move;

        public BattleEventArgs(BattleEventType type, ActiveMonster monster)
        {
            Type = type;
            Monster = monster;
        }

        public BattleEventArgs(BattleEventType type, ActiveMonster monster, Move move)
        {
            Type = type;
            Monster = monster;
            Move = move;
        }

        public BattleEventArgs(BattleEventType type, ActiveMonster monster, int hpBefore, int hpAfter)
        {
            Type = type;
            Monster = monster;
            HPBefore = hpBefore;
            HPAfter = hpAfter;
        }
    }
}
