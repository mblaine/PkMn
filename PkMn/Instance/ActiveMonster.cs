using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;

namespace PkMn.Instance
{
    public class ActiveMonster
    {
        public Trainer Trainer;
        public Monster Monster;
        public BattleStats StatStages;
        public BattleStats EffectiveStats;
        public int MoveIndex;
        public bool IsConfused;
        public bool IsSemiInvulnerable;
        public bool Flinched;
        public Move QueuedMove;

        public Move SelectedMove
        {
            get 
            {
                if (Monster == null)
                    return null;
                if (QueuedMove != null)
                    return QueuedMove;
                if (MoveIndex < 0 || MoveIndex > 3)
                    return null;
                return Monster.Moves[MoveIndex];
            }
        }

        public ActiveMonster(Trainer trainer, Monster monster)
        {
            Trainer = trainer;
            Monster = monster;
            StatStages = new BattleStats();
            EffectiveStats = new BattleStats();
            MoveIndex = -1;
            IsConfused = false;
            Flinched = false;
            IsSemiInvulnerable = false;
            QueuedMove = null;
            Recalc();
        }

        public void Reset()
        {
            StatStages = new BattleStats();
            EffectiveStats = new BattleStats();
            IsConfused = false;
            IsSemiInvulnerable = false;
            Flinched = false;
            MoveIndex = -1;
            QueuedMove = null;
        }

        public void Recalc()
        {
            EffectiveStats.Attack = RecalcStat(Monster.Stats.Attack, StatStages.Attack);
            EffectiveStats.Defense = RecalcStat(Monster.Stats.Defense, StatStages.Defense);
            EffectiveStats.Special = RecalcStat(Monster.Stats.Special, StatStages.Special);
            EffectiveStats.Speed = RecalcStat(Monster.Stats.Speed, StatStages.Speed);
            EffectiveStats.Attack = RecalcStat(Monster.Stats.Attack, StatStages.Attack);
            EffectiveStats.Evade = RecalcStat(100, StatStages.Evade);
            EffectiveStats.Accuracy = RecalcStat(100, StatStages.Accuracy);
        }

        protected int RecalcStat(int value, int stage)
        {
            int ret;
            if (stage >= 0)
                ret = (int)(value * (1m + 0.5m * stage));
            else
                ret = (int)(value * (2m / (2m + Math.Abs(stage))));

            if (ret < 1)
                return 1;
            else if (ret > 999)
                return 999;
            else
                return ret;
        }
    }
}
