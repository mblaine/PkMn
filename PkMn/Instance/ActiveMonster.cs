using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;
using PkMn.Model.Enums;
using PkMn.Model.Moves;

namespace PkMn.Instance
{
    public class ActiveMonster
    {
        public Trainer Trainer;
        public Monster Monster;
        public BattleStats StatStages;
        public BattleStats EffectiveStats;
        
        public int MoveIndex;
        public int BadlyPoisonedCount;
        public int ConfusedCount;
        
        public int DisabledMoveIndex;
        public int DisabledCount;
        
        public bool IsSemiInvulnerable;
        public bool Flinched;
        public bool MoveCancelled;
        public bool IsSeeded;
        
        public Move QueuedMove;
        public int QueuedMoveLimit;
        public int QueuedMoveDamage;

        public Element Type1Override;
        public Element Type2Override;
        public Stats StatsOverride;
        public Move[] MovesOverride;

        public Move MoveOverrideTemporary;
        public Move LastMoveUsed;
        public int AccumulatedDamage;
        public int? SubstituteHP;

        public int DefenseMultiplier;
        public int SpecialDefenseMultiplier;

        public bool ProtectStages;

        public Move SelectedMove
        {
            get 
            {
                if (Monster == null)
                    return null;
                if (MoveOverrideTemporary != null)
                    return MoveOverrideTemporary;
                if (QueuedMove != null)
                    return QueuedMove;
                if (MoveIndex < 0 || MoveIndex > 3)
                    return null;
                return Moves[MoveIndex];
            }
        }

        public bool IsConfused
        {
            get
            {
                return ConfusedCount > 0;
            }
        }

        public Element Type1 { get { return Type1Override ?? Monster.Species.Type1; } }
        public Element Type2 { get { return Type1Override != null ? Type2Override : (Type2Override ?? Monster.Species.Type2); } }
        public Stats Stats { get { return StatsOverride ?? Monster.Stats; } }

        public Move[] Moves
        {
            get
            {
                if (MovesOverride == null)
                    return Monster.Moves;
                else if (MovesOverride.Length == 1)
                    return Monster.Moves.Select(m => m.Effects.Any(e => e.Type == MoveEffectType.Copy && ((CopyEffect)e).What == "move") ? MovesOverride[0] : m).ToArray();
                else
                    return MovesOverride;
            }
        }

        public ActiveMonster(Trainer trainer, Monster monster)
        {
            Trainer = trainer;
            Monster = monster;

            if (monster.Status == StatusCondition.BadlyPoisoned)
                monster.Status = StatusCondition.Poison;

            Reset();
            Recalc();
        }

        public void Reset()
        {
            StatStages = new BattleStats();
            EffectiveStats = new BattleStats();
            IsSemiInvulnerable = false;
            Flinched = false;
            MoveCancelled = false;
            ConfusedCount = 0;
            BadlyPoisonedCount = 1;
            MoveIndex = -1;
            DisabledMoveIndex = -1;
            DisabledCount = 0;
            QueuedMove = null;
            QueuedMoveLimit = -1;
            QueuedMoveDamage = -1;
            Type1Override = null;
            Type2Override = null;
            StatsOverride = null;
            MovesOverride = null;
            MoveOverrideTemporary = null;
            LastMoveUsed = null;
            AccumulatedDamage = 0;
            DefenseMultiplier = 1;
            SpecialDefenseMultiplier = 1;
            ProtectStages = false;
            IsSeeded = false;
            SubstituteHP = null;
        }

        public void Recalc(StatType? stat = null)
        {
            if (stat != null)
            {
                if(stat == StatType.Evade || stat == StatType.Accuracy)
                    EffectiveStats[(StatType)stat] = RecalcStat(100, StatStages[(StatType)stat]);
                else if (stat != StatType.CritRatio)
                    EffectiveStats[(StatType)stat] = RecalcStat(Stats[(StatType)stat], StatStages[(StatType)stat]);
            }
            else
            {
                EffectiveStats.Attack = RecalcStat(Stats.Attack, StatStages.Attack);
                EffectiveStats.Defense = RecalcStat(Stats.Defense, StatStages.Defense);
                EffectiveStats.Special = RecalcStat(Stats.Special, StatStages.Special);
                EffectiveStats.Speed = RecalcStat(Stats.Speed, StatStages.Speed);
                EffectiveStats.Attack = RecalcStat(Stats.Attack, StatStages.Attack);
                EffectiveStats.Evade = RecalcStat(100, StatStages.Evade);
                EffectiveStats.Accuracy = RecalcStat(100, StatStages.Accuracy);
            }
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

        public override string ToString()
        {
            return Monster.ToString();
        }
    }
}
