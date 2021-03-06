﻿using System;
using System.Collections.Generic;
using System.Linq;
using PkMn.Model;
using PkMn.Model.Enums;
using PkMn.Model.MoveEffects;

namespace PkMn.Instance
{
    public partial class Battle
    {
        protected void HandleFainting(ActiveMonster current, bool isPlayer, ActiveMonster opponent)
        {
            if (current.Monster.CurrentHP == 0)
            {
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonFainted, current));
                current.Monster.Status = StatusCondition.Faint;
                OnSendMessage("{0}{1} fainted!", current.Trainer.MonNamePrefix, current.Monster.Name);

                if (!current.AnyMonstersRemaining)
                {
                    current.Monster = null;

                    if (isPlayer)
                        OnSendMessage("{0} is out of usable Pokémon! {0} blacked out!", current.Trainer.Name);
                    else
                        OnSendMessage("{0} defeated {1}!", this.Player.Name, current.Trainer.Name);

                    if (RewardMoney > 0)
                        OnSendMessage("{0} picked up ${1}!", isPlayer ? Foe.Name : Player.Name, RewardMoney);

                    return;
                }

                bool playerShifted = false;
                Monster playerNextMon = null;

                if (isPlayer)
                {
                    Monster mon = PlayerChooseNextMon(current.Trainer, false);

                    while (mon.CurrentHP <= 0)
                    {
                        OnSendMessage("There's no will to fight!");
                        mon = PlayerChooseNextMon(current.Trainer, false);
                    }

                    current.Monster = mon;
                }
                else
                {
                    Monster aboutToUse = null;

                    if (FoeChooseNextMon != null)
                        aboutToUse = FoeChooseNextMon(current.Trainer, false);
                    else
                        aboutToUse = current.Trainer.Party.Where(m => m != null && m.CurrentHP > 0 && m.Status != StatusCondition.Faint).FirstOrDefault();

                    if (Shift)
                    {
                        OnSendMessage("{0} is about to use {1}!", current.Trainer.Name, aboutToUse.Name);
                        OnSendMessage("Will {0} change Pokémon?", opponent.Trainer.Name);
                        playerNextMon = PlayerChooseNextMon(opponent.Trainer, true);

                        while (playerNextMon != null && playerNextMon != opponent.Monster && playerNextMon.CurrentHP <= 0)
                        {
                            OnSendMessage("There's no will to fight!");
                            playerNextMon = PlayerChooseNextMon(current.Trainer, true);
                        }

                        if (playerNextMon != null && playerNextMon != opponent.Monster)
                        {
                            playerShifted = true;
                        }
                    }

                    if (current.Monster.CurrentHP <= 0)
                        current.Monster.Status = StatusCondition.Faint;

                    current.Monster = aboutToUse;
                }

                if (isPlayer)
                    OnSendMessage(GetPlayerSentOutText(), current.Monster.Name);
                else
                    OnSendMessage("{0} sent out {1}!", current.Trainer.Name, current.Monster.Name);

                OnBattleEvent(new BattleEventArgs(BattleEventType.MonSentOut, current));

                if (playerShifted)
                {
                    OnSendMessage(GetPlayerRecalledText(), opponent.Monster.Name);
                    OnBattleEvent(new BattleEventArgs(BattleEventType.MonRecalled, opponent));
                    opponent.Monster = playerNextMon;
                    OnSendMessage(GetPlayerSentOutText(), opponent.Monster.Name);
                    OnBattleEvent(new BattleEventArgs(BattleEventType.MonSentOut, opponent));
                }

                //cancel trapping move that isn't rage
                if (opponent.QueuedMove != null && opponent.QueuedMove.Effects.Any(e => e.Type == MoveEffectType.LockInMove && ((LockInEffect)e).ConstantDamage))
                {
                    opponent.QueuedMove = null;
                    opponent.QueuedMoveDamage = -1;
                    opponent.QueuedMoveLimit = -1;
                }
            }
        }

        protected void HandleDamageOverTime(ActiveMonster current, ActiveMonster opponent)
        {
            if (current.Monster.Status == StatusCondition.Poison || current.Monster.Status == StatusCondition.BadlyPoisoned)
            {
                OnSendMessage("{0}{1}'s hurt by poison!", current.Trainer.MonNamePrefix, current.Monster.Name);
                OnBattleEvent(new BattleEventArgs(BattleEventType.StatusAilment, current, StatusCondition.Poison));
                int damage = (int)(((decimal)current.Monster.Stats.HP) / 16m);
                if (current.Monster.Status == StatusCondition.BadlyPoisoned)
                    damage = damage * current.BadlyPoisonedCount++;
                int oldHP = current.Monster.CurrentHP;
                int newHP = Math.Max(0, current.Monster.CurrentHP - damage);
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, newHP));
                current.Monster.CurrentHP = newHP;
                current.AccumulatedDamage += damage;
                OnSendDebugMessage("Did {0} damage to {1}{2}", damage, current.Trainer.MonNamePrefix, current.Monster.Name);
            }
            else if (current.Monster.Status == StatusCondition.Burn)
            {
                OnSendMessage("{0}{1}'s hurt by the burn!", current.Trainer.MonNamePrefix, current.Monster.Name);
                OnBattleEvent(new BattleEventArgs(BattleEventType.StatusAilment, current, StatusCondition.Burn));

                int damage = (int)(((decimal)current.Monster.Stats.HP) / 16m);
                int oldHP = current.Monster.CurrentHP;
                int newHP = Math.Max(0, current.Monster.CurrentHP - damage);
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, newHP));
                current.Monster.CurrentHP = newHP;
                current.AccumulatedDamage += damage;
                OnSendDebugMessage("Did {0} damage to {1}{2}", damage, current.Trainer.MonNamePrefix, current.Monster.Name);
            }

            if (current.IsSeeded)
            {
                int damage = (int)(((decimal)current.Monster.Stats.HP) / 16m);
                if (damage == 0)
                    damage = 1;
                if (current.Monster.Status == StatusCondition.BadlyPoisoned)
                    damage = damage * current.BadlyPoisonedCount;

                OnBattleEvent(new BattleEventArgs(BattleEventType.StatusAilment, current, StatusCondition.Seeded));

                int oldHP = current.Monster.CurrentHP;
                int newHP = Math.Max(0, current.Monster.CurrentHP - damage);
                OnSendDebugMessage("Did {0} damage to {1}{2}", damage, current.Trainer.MonNamePrefix, current.Monster.Name);
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, newHP));
                current.Monster.CurrentHP = newHP;

                oldHP = opponent.Monster.CurrentHP;               
                int hpRestored = Math.Min(damage, opponent.Monster.Stats.HP - opponent.Monster.CurrentHP);
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, opponent, oldHP, oldHP + hpRestored));
                opponent.Monster.CurrentHP += hpRestored;
                OnSendDebugMessage("Restored {0} HP to {1}{2}", hpRestored, opponent.Trainer.MonNamePrefix, opponent.Monster.Name);

                OnSendMessage("LEECH SEED saps {0}{1}!", current.Trainer.MonNamePrefix, current.Monster.Name);
                
            }
        }

        public bool HandleStatEffect(ActiveMonster current, ActiveMonster opponent, StatEffect eff, bool hitOpponent, bool showFailMessage = true)
        {
            bool ret = false;

            if (Rng.Next(0, 256) < eff.Chance)
            {
                ActiveMonster[] mons = new ActiveMonster[] { current, opponent };

                for (int i = 0; i < mons.Length; i++)
                {
                    if (mons[i] == null)
                        continue;

                    Who who = i == 0 ? Who.Self : Who.Foe;

                    if (eff.Who == Who.Foe && who == Who.Foe && opponent.SubstituteHP > 0)
                    {
                        if (showFailMessage)
                            messageBuffer.AppendLine("But, it failed!");
                        continue;
                    }

                    if (mons[i] != null && (eff.Who == who || eff.Who == Who.Both) && (i == 0 || hitOpponent))
                    {
                        if (i == 1 && mons[i].ProtectStages)
                        {
                            if (showFailMessage)
                                messageBuffer.AppendLine("But, it failed!");
                        }
                        else if (eff.Temporary)
                        {
                            mons[i].EffectiveStats[eff.Stat] = (int)(((decimal)mons[i].EffectiveStats[eff.Stat]) * eff.Multiplier);
                        }
                        else if (eff.Condition == "defense-only")
                        {
                            bool worked = false;
                            if (eff.Stat == StatType.Defense && mons[i].DefenseMultiplier == 1)
                            {
                                mons[i].DefenseMultiplier = (int)eff.Multiplier;
                                worked = true;
                            }
                            else if (eff.Stat == StatType.Special && mons[i].SpecialDefenseMultiplier == 1)
                            {
                                mons[i].SpecialDefenseMultiplier = (int)eff.Multiplier;
                                worked = true;
                            }
                            else
                                messageBuffer.AppendLine("But, it failed!");

                            if (worked && !string.IsNullOrWhiteSpace(eff.Message))
                                messageBuffer.AppendLine(string.Format(eff.Message, mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));

                            if (worked)
                                ret = true;
                        }
                        else
                        {
                            if ((eff.Change > 0 && mons[i].StatStages[eff.Stat] >= 6) || (eff.Change < 0 && mons[i].StatStages[eff.Stat] <= -6))
                            {
                                if (showFailMessage)
                                    messageBuffer.AppendLine("Nothing happened!");
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(eff.Message))
                                    messageBuffer.AppendLine(string.Format(eff.Message, mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));

                                if (eff.Stat == StatType.CritRatio)
                                {
                                    mons[i].EffectiveStats.CritRatio = eff.Constant;
                                }
                                else
                                {
                                    mons[i].StatStages[eff.Stat] += eff.Change;
                                    if (mons[i].StatStages[eff.Stat] > 6)
                                        mons[i].StatStages[eff.Stat] = 6;
                                    else if (mons[i].StatStages[eff.Stat] < -6)
                                        mons[i].StatStages[eff.Stat] = -6;
                                    messageBuffer.AppendLine(string.Format("{0}{1}'s {2} {3}{4}!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name, eff.Stat.ToString().ToUpper(), eff.Change > 1 ? "greatly " : eff.Change < -1 ? "sharply " : "", eff.Change > 0 ? "rose" : "fell"));
                                    mons[i].Recalc(eff.Stat);
                                }

                                ret = true;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public bool HandleStatusEffect(ActiveMonster current, Move move, ActiveMonster opponent, StatusEffect eff, bool hitOpponent)
        {
            if (eff.Type != MoveEffectType.Status)
                return false;

            bool ret = false;

            if (Rng.Next(0, 256) < eff.Chance)
            {
                ActiveMonster[] mons = new ActiveMonster[] { current, opponent };

                for (int i = 0; i < mons.Length; i++)
                {
                    if (mons[i] == null)
                        continue;

                    Who who = i == 0 ? Who.Self : Who.Foe;

                    if (eff.Who == Who.Foe && who == Who.Foe && opponent.SubstituteHP > 0)
                    {
                        if (move.Category == ElementCategory.Status)
                            messageBuffer.AppendLine(string.Format("It didn't affect {0}{1}.", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                        continue;
                    }

                    if (mons[i] != null && (eff.Who == who || eff.Who == Who.Both) && (i == 0 || hitOpponent))
                    {
                        if (eff.Status == StatusCondition.Faint)
                        {
                            mons[i].Monster.CurrentHP = 0;
                            ret = true;
                        }
                        else if (eff.Status == StatusCondition.Confusion)
                        {
                            if (mons[i].IsConfused)
                                messageBuffer.AppendLine(string.Format("{0}{1} is already confused!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                            else
                            {
                                messageBuffer.AppendLine(string.Format(eff.Message ?? "{0}{1} became confused!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                                mons[i].ConfusedCount = Rng.Next(2, 6);
                                ret = true;
                            }
                        }
                        else if (eff.Status == StatusCondition.Flinch)
                        {
                            mons[i].Flinched = true;
                            ret = true;
                        }
                        else if ((eff.Force || mons[i].Monster.Status == StatusCondition.None) && !mons[i].Species.IsImmuneToStatus(eff.Status))
                        {
                            string message = eff.Message;
                            if (mons[i].Monster.Status != StatusCondition.None && eff.Force && !string.IsNullOrEmpty(eff.ForceMessage))
                                message = eff.ForceMessage;

                            mons[i].Monster.Status = eff.Status;
                            ret = true;

                            switch (eff.Status)
                            {
                                case StatusCondition.Paralysis:
                                    messageBuffer.AppendLine(string.Format(message ?? "{0}{1} was paralyzed!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                                    mons[i].EffectiveStats.Speed = (int)(((decimal)mons[i].EffectiveStats.Speed) * 0.25m);
                                    break;
                                case StatusCondition.Sleep:
                                    messageBuffer.AppendLine(string.Format(message ?? "{0}{1} fell asleep!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                                    mons[i].Monster.SleepCounter = eff.TurnLimit > 0 ? eff.TurnLimit : Rng.Next(1, 8);
                                    break;
                                case StatusCondition.Burn:
                                    messageBuffer.AppendLine(string.Format(message ?? "{0}{1} was burned!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                                    mons[i].EffectiveStats.Attack = (int)(((decimal)mons[i].EffectiveStats.Attack) * 0.5m);
                                    break;
                                case StatusCondition.BadlyPoisoned:
                                    messageBuffer.AppendLine(string.Format(message ?? "{0}{1} was badly poisoned!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                                    break;
                                case StatusCondition.Freeze:
                                    messageBuffer.AppendLine(string.Format(message ?? "{0}{1} was frozen!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                                    break;
                                default:
                                    messageBuffer.AppendLine(string.Format(message ?? "{0}{1} was {2}ed!", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name, eff.Status.ToString().ToLower()));
                                    break;
                            }

                        }
                        else if (move.Category == ElementCategory.Status)
                            messageBuffer.AppendLine(string.Format("It didn't affect {0}{1}.", mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                    }
                }
            }

            return ret;
        }

        protected bool HandleDisableEffect(ActiveMonster current, ActiveMonster opponent)
        {
            if (opponent.DisabledCount > 0)
            {
                messageBuffer.AppendLine("But, it failed!");
                return false;
            }
            else
            {
                Move[] enabled = opponent.Moves.Zip(opponent.CurrentPP, (move, pp) => new KeyValuePair<Move, int>(move, pp)).Where(p => p.Value > 0).Select(p => p.Key).ToArray();

                if (enabled.Length <= 0)
                {
                    messageBuffer.AppendLine("But, it failed!");
                    return false;
                }
                else
                {
                    Move disabledMove = enabled[Rng.Next(0, enabled.Length)];

                    for (int i = 0; i < opponent.Moves.Length; i++)
                    {
                        if (opponent.Moves[i] == disabledMove)
                        {
                            opponent.DisabledMoveIndex = i;
                            break;
                        }
                    }

                    MultiEffect disableEffect = (MultiEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Disable).First();

                    opponent.DisabledCount = Rng.Next(disableEffect.Min, disableEffect.Max + 1);

                    messageBuffer.AppendLine(string.Format("{0}{1}'s {2} was disabled!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name, disabledMove.Name.ToUpper()));
                    return true;
                }
            }
        }

        protected void HandleLockInBeginning(ActiveMonster current, MultiEffect lockInEffect, ActiveMonster opponent)
        {
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.CancelEnemyMove))
                OnSendMessage("{0}{1} can't move!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
            current.QueuedMove = current.SelectedMove;
            if (lockInEffect.Min == 2 && lockInEffect.Max == 5)
                current.QueuedMoveLimit = new int[] { 2, 2, 2, 3, 3, 3, 4, 5 }[Rng.Next(0, 8)];
            else
                current.QueuedMoveLimit = Rng.Next(lockInEffect.Min, lockInEffect.Max + 1);

            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.CustomDamage && ((CustomDamageEffect)e).Calculation == "accumulated-on-lock-end"))
                current.AccumulatedDamage = 0;

            OnSendDebugMessage("{0}{1} locked in for {2} moves", current.Trainer.MonNamePrefix, current.Monster.Name, current.QueuedMoveLimit);
        }

        protected void HandleLockInEnding(ActiveMonster current, ActiveMonster opponent, bool moveHit)
        {
            foreach (StatusEffect eff in current.QueuedMove.Effects.Where(e => e.Type == MoveEffectType.Status).Cast<StatusEffect>().Where(e => e.Condition == "lock-in-end"))
            {
                HandleStatusEffect(current, current.QueuedMove, opponent, eff, moveHit);
                ClearMessageBuffer();
            }

            current.QueuedMoveLimit = -1;
            current.QueuedMoveDamage = -1;
            current.QueuedMove = null;
        }

        protected bool HandleResetStatusEffect(ActiveMonster current, StatusEffect eff, ActiveMonster opponent)
        {
            if (eff.Type != MoveEffectType.ResetStatus)
                return false;

            ActiveMonster[] mons = new ActiveMonster[] { current, opponent };
            bool ret = false;

            for (int i = 0; i < mons.Length; i++)
            {
                Who who = i == 0 ? Who.Self : Who.Foe;

                if (eff.Who == Who.Both || eff.Who == who)
                {
                    if (eff.Status == StatusCondition.Confusion)
                    {
                        mons[i].ConfusedCount = 0;
                        ret = true;
                    }
                    else if (eff.Status == StatusCondition.Flinch)
                    {
                        mons[i].Flinched = false;
                        ret = true;
                    }
                    else if (eff.Status == StatusCondition.BadlyPoisoned && mons[i].Monster.Status == StatusCondition.BadlyPoisoned)
                    {
                        mons[i].Monster.Status = StatusCondition.Poison;
                        mons[i].BadlyPoisonedCount = 0;
                        ret = true;
                    }
                    else if (eff.Status == mons[i].Monster.Status)
                    {
                        if (i == 1 && (mons[i].Monster.Status == StatusCondition.Sleep || mons[i].Monster.Status == StatusCondition.Freeze))
                            mons[i].MoveCancelled = true;
                        mons[i].Monster.Status = StatusCondition.None;
                        ret = true;
                    }
                    else if (eff.Status == StatusCondition.All)
                    {
                        if (i == 1 && (mons[i].Monster.Status == StatusCondition.Sleep || mons[i].Monster.Status == StatusCondition.Freeze))
                            mons[i].MoveCancelled = true;
                        mons[i].ConfusedCount = 0;
                        mons[i].Flinched = false;
                        mons[i].Monster.Status = StatusCondition.None;
                        mons[i].BadlyPoisonedCount = 0;
                        ret = true;
                    }
                }
            }

            return ret;
        }

        protected void HandleResetStatStageEffect(ActiveMonster current, StatStageEffect eff, ActiveMonster opponent)
        {
            ActiveMonster[] mons = new ActiveMonster[] { current, opponent };

            for (int i = 0; i < mons.Length; i++)
            {
                Who who = i == 0 ? Who.Self : Who.Foe;

                if (eff.Who == Who.Both || eff.Who == who)
                {
                    mons[i].StatStages = new BattleStats();
                    mons[i].DefenseMultiplier = 1;
                    mons[i].SpecialDefenseMultiplier = 1;
                    mons[i].ProtectStages = false;
                    mons[i].IsSeeded = false;
                    mons[i].Recalc();
                }
            }
        }

        protected void HandleMissDamage(ActiveMonster current, ExtraDamageEffect crashEffect)
        {
            int crashDamage = crashEffect.Value;
            int oldHP = current.Monster.CurrentHP;
            crashDamage = Math.Min(crashDamage, current.Monster.CurrentHP);
            OnSendMessage(crashEffect.Message ?? "{0}{1} got hurt!", current.Trainer.MonNamePrefix, current.Monster.Name);
            OnBattleEvent(new BattleEventArgs(BattleEventType.MonCrashed, current));
            OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, oldHP - crashDamage));
            current.Monster.CurrentHP -= crashDamage;
            current.AccumulatedDamage += crashDamage;
            OnSendDebugMessage("Did {0} damage to {1}{2}", crashDamage, current.Trainer.MonNamePrefix, current.Monster.Name);

        }

        protected bool HandleProtectStatStageEffect(ActiveMonster current, StatStageEffect eff, ActiveMonster opponent)
        {
            ActiveMonster[] mons = new ActiveMonster[] { current, opponent };

            bool ret = false;

            for (int i = 0; i < mons.Length; i++)
            {
                Who who = i == 0 ? Who.Self : Who.Foe;

                if (eff.Who == Who.Both || eff.Who == who)
                {
                    if (mons[i].ProtectStages)
                        messageBuffer.AppendLine("But, it failed!");
                    else if (!string.IsNullOrEmpty(eff.Message))
                    {
                        messageBuffer.AppendLine(string.Format(eff.Message, mons[i].Trainer.MonNamePrefix, mons[i].Monster.Name));
                        ret = true;
                    }

                    current.ProtectStages = true;
                }
            }

            return ret;
        }

        protected void HandleCopyEffect(ActiveMonster current, CopyEffect copy, ActiveMonster opponent)
        {
            if (copy.What == "move")
            {
                int copyIndex;
                if (current.Trainer.IsPlayer)
                    copyIndex = PlayerChooseMoveToMimic(opponent.Moves);
                else
                {
                    if (FoeChooseMove != null)
                        copyIndex = FoeChooseMove(opponent.Moves);
                    else
                        copyIndex = Rng.Next(0, opponent.Moves.Count(m => m != null));
                }

                if (current.MovesOverride != null && current.MovesOverride.Length > 1)
                    current.MovesOverride[current.MoveIndex] = opponent.Moves[copyIndex];
                else
                    current.MovesOverride = new Move[] { opponent.Moves[copyIndex] };
                if (!string.IsNullOrEmpty(copy.Message))
                    OnSendMessage(copy.Message, current.Trainer.MonNamePrefix, current.Monster.Name, current.SelectedMove.Name.ToUpper());
            }
            else if (copy.What == "type")
            {
                current.Type1Override = opponent.Type1;
                current.Type2Override = opponent.Type2;
                if (!string.IsNullOrEmpty(copy.Message))
                    OnSendMessage(copy.Message, opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
            }
            else if (copy.What == "all-moves")
            {
                current.MovesOverride = (Move[])opponent.Moves.Clone();
                current.CurrentPPOverride = new int[] { 5, 5, 5, 5 };
            }
            else if (copy.What == "stat-stages")
            {
                current.StatStages = new BattleStats(opponent.StatStages);
            }
            else if (copy.What == "stats")
            {
                current.StatsOverride = new Stats(opponent.Stats);
                current.EffectiveStats = new BattleStats(opponent.EffectiveStats);
            }
            else if (copy.What == "species")
            {
                current.SpeciesOverride = opponent.Species;
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonTransformed, current));
                if (!string.IsNullOrEmpty(copy.Message))
                    OnSendMessage(copy.Message, current.Trainer.MonNamePrefix, current.Monster.Name, opponent.Monster.Species.Name.ToUpper());
            }
        }

        protected bool HandleRestoreHealthEffect(ActiveMonster current, HealthEffect eff)
        {
            if (eff.Type != MoveEffectType.RestoreHealth)
                return false;

            if (eff.Of == "max" && eff.Who == Who.Self)
            {
                int hpRestored = (int)(eff.Percent / 100m * (decimal)current.Monster.Stats.HP);
                if (hpRestored == 0)
                    hpRestored = 1;
                hpRestored = Math.Min(hpRestored, current.Monster.Stats.HP - current.Monster.CurrentHP);
                OnSendDebugMessage("Restored {0} HP to {1}{2}", hpRestored, current.Trainer.MonNamePrefix, current.Monster.Name);
                int oldHP = current.Monster.CurrentHP;
                int newHP = current.Monster.CurrentHP + hpRestored;
                OnBattleEvent(new BattleEventArgs(BattleEventType.AttackHit, current, current.SelectedMove));
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, newHP));
                current.Monster.CurrentHP += hpRestored;
                
                messageBuffer.AppendLine(string.Format("{0}{1} regained health!", current.Trainer.MonNamePrefix, current.Monster.Name));
                return true;
            }
            //nothing else to implement
            return false;
        }

        protected void HandleTransferHealthEffect(ActiveMonster current, HealthEffect eff, ActiveMonster opponent, int damage)
        {
            if (eff.Type != MoveEffectType.TransferHealth)
                return;

            if (eff.Of == "damage")
            {
                int hpRestored = (int)(eff.Percent / 100m * (decimal)damage);
                if (hpRestored == 0)
                    hpRestored = 1;
                int oldHP = current.Monster.CurrentHP;
                hpRestored = Math.Min(hpRestored, current.Monster.Stats.HP - current.Monster.CurrentHP);
                OnSendDebugMessage("Restored {0} HP to {1}{2}", hpRestored, current.Trainer.MonNamePrefix, current.Monster.Name);
                OnSendMessage(eff.Message ?? "Sucked health from {0}{1}!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, oldHP + hpRestored));
                current.Monster.CurrentHP += hpRestored;
                
            }
        }

        protected void HandleRecoilEffect(ActiveMonster current, ExtraDamageEffect eff, int damage)
        {
            if (eff.Type != MoveEffectType.RecoilDamage)
                return;
            int recoilDamage = (int)(eff.Percent / 100m * (decimal)damage);
            if (recoilDamage == 0)
                recoilDamage = 1;
            int oldHP = current.Monster.CurrentHP;
            recoilDamage = Math.Min(recoilDamage, current.Monster.CurrentHP);
            OnSendMessage("{0}{1}'s hit with recoil!", current.Trainer.MonNamePrefix, current.Monster.Name);
            OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, oldHP - recoilDamage));
            current.Monster.CurrentHP -= recoilDamage;
            current.AccumulatedDamage += recoilDamage;
            OnSendDebugMessage("Did {0} damage to {1}{2}", recoilDamage, current.Trainer.MonNamePrefix, current.Monster.Name);
        }

        protected bool HandleSubstituteEffect(ActiveMonster current)
        {
            if (current.SubstituteHP > 0)
            {
                
                messageBuffer.AppendFormat("{0}{1} has a SUBSTITUTE!", current.Trainer.MonNamePrefix, current.Monster.Name).AppendLine();
                return false;
            }

            int hpCost = current.Monster.Stats.HP / 4;

            if (current.Monster.CurrentHP < hpCost)
            {
                messageBuffer.AppendLine("Too weak to make a SUBSTITUTE!");
                return false;
            }
            int oldHP = current.Monster.CurrentHP;
            hpCost = Math.Min(hpCost, current.Monster.CurrentHP);
            messageBuffer.AppendLine("It created a SUBSTITUTE!");
            OnSendDebugMessage("Did {0} damage to {1}{2}", hpCost, current.Trainer.MonNamePrefix, current.Monster.Name);
            OnBattleEvent(new BattleEventArgs(BattleEventType.MonHPChanged, current, oldHP, oldHP - hpCost));
            current.Monster.CurrentHP -= hpCost;
            current.SubstituteHP = hpCost + 1;
            return true;
        }
    }
}
