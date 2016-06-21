using System;
using PkMn.Model;
using PkMn.Model.Enums;

namespace PkMn.Instance
{
    public class BattleEventArgs : EventArgs
    {
        public readonly BattleEventType Type;
        public readonly ActiveMonster Monster;
        public readonly int HPBefore;
        public readonly int HPAfter;
        public readonly Move Move;
        public readonly StatusCondition Status;

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

        public BattleEventArgs(BattleEventType type, ActiveMonster monster, StatusCondition status)
        {
            Type = type;
            Monster = monster;
            Status = status;
        }

    }
}
