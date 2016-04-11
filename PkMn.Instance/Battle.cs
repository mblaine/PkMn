using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;
using PkMn.Model.Enums;
using PkMn.Model.MoveEffects;

namespace PkMn.Instance
{
    public partial class Battle
    {
        public delegate Monster ChooseMonEventHandler(Trainer trainer);
        public delegate void SendMessageEventHandler(string message);
        public delegate BattleAction ChooseActionEventHandler(ActiveMonster current, Trainer trainer);
        public delegate int ChooseMoveEventHandler(Move[] moeves);

        protected Trainer Player;
        protected Trainer Foe;

        public ActiveMonster PlayerCurrent { get; protected set; }
        public ActiveMonster FoeCurrent { get; protected set; }
        protected bool IsWildBattle;

        protected int runCount;

        public ChooseMonEventHandler ChooseNextMon;
        public SendMessageEventHandler SendMessage;
        public ChooseActionEventHandler ChooseAction;
        public ChooseMoveEventHandler ChooseMoveToMimic;

        public int LastDamageDealt;
        public int RewardMoney;

        public Battle(Trainer player, Trainer foe, bool isWildBattle)
        {
            Player = player;
            Foe = foe;
            IsWildBattle = isWildBattle;
            runCount = 0;
            RewardMoney = 0;

            foreach (Monster mon in Player.Party)
            {
                if (mon != null && mon.Status != StatusCondition.Faint)
                {
                    PlayerCurrent = new ActiveMonster(Player, mon);
                    break;
                }
            }

            foreach (Monster mon in Foe.Party)
            {
                if (mon != null && mon.Status != StatusCondition.Faint)
                {
                    FoeCurrent = new ActiveMonster(Foe, mon);
                    break;
                }
            }

            if (PlayerCurrent == null || FoeCurrent == null)
                throw new Exception();
        }

        protected void OnSendMessage(string message, params Object[] args)
        {
            if (SendMessage != null)
                SendMessage(string.Format(message, args));
        }

        public bool Step()
        {
            if (PlayerCurrent.Monster.CurrentHP == 0)
            {
                HandleFainting(PlayerCurrent, true, FoeCurrent);
                return PlayerCurrent.Monster != null;
            }

            if (FoeCurrent.Monster.CurrentHP == 0)
            {
                HandleFainting(FoeCurrent, false, PlayerCurrent);
                return FoeCurrent.Monster != null;
            }

            BattleAction playerAction = null;
            PlayerCurrent.MoveIndex = -1;
            FoeCurrent.MoveIndex = -1;

            bool playerCanAttack = PlayerCurrent.CanSelectAMove;
            bool foeCanAttack = FoeCurrent.CanSelectAMove;
            if (!playerCanAttack)
                PlayerCurrent.MoveOverrideTemporary = Move.Moves["Struggle"];

            if (PlayerCurrent.QueuedMove == null  && PlayerCurrent.MoveOverrideTemporary == null)
            {
                playerAction = ChooseAction(PlayerCurrent, Player);

                while (playerAction.Type == BattleActionType.UseMove && ((PlayerCurrent.DisabledCount > 0 && playerAction.WhichMove == PlayerCurrent.DisabledMoveIndex) || PlayerCurrent.CurrentPP[playerAction.WhichMove] <= 0))
                {
                    //OnSendMessage("It's disabled");
                    playerAction = ChooseAction(PlayerCurrent, Player);
                }

                if (playerAction.Type == BattleActionType.Run)
                {
                    runCount++;

                    if (IsWildBattle)
                    {

                        int B = FoeCurrent.EffectiveStats.Speed / 4 % 256;
                        if (B == 0)
                        {
                            OnSendMessage("{0} successfully ran!", Player.Name);
                            return false;
                        }

                        int F = (int)Math.Floor(PlayerCurrent.EffectiveStats.Speed * 32m / B + 30m * runCount);
                        if (F > 255)
                        {
                            OnSendMessage("{0} successfully ran!", Player.Name);
                            return false;
                        }
                        else
                        {
                            if (Rng.Next(0, 256) < F)
                            {
                                OnSendMessage("{0} successfully ran!", Player.Name);
                                return false;
                            }
                            else
                            {
                                OnSendMessage("Unable to run away!");
                            }
                        }
                    }
                    else
                        OnSendMessage("You can't run from a trainer battle!");
                }
                else
                    runCount = 0;

                switch (playerAction.Type)
                {
                    case BattleActionType.UseItem:
                        throw new Exception();
                    case BattleActionType.ChangeMon:
                        OnSendMessage("Come back {0}!", PlayerCurrent.Monster.Name);
                        PlayerCurrent.Monster = playerAction.SwitchTo;
                        PlayerCurrent.Reset();
                        OnSendMessage("Go {0}!", PlayerCurrent.Monster.Name);
                        break;
                    case BattleActionType.UseMove:
                        PlayerCurrent.MoveIndex = playerAction.WhichMove;
                        break;
                }
            }

            if (!foeCanAttack)
                FoeCurrent.MoveOverrideTemporary = Move.Moves["Struggle"];

            if(FoeCurrent.QueuedMove == null && FoeCurrent.MoveOverrideTemporary == null)
                FoeCurrent.MoveIndex = Rng.Next(0, FoeCurrent.Moves.Count(m => m != null));

            ActiveMonster first = WhoGoesFirst(PlayerCurrent, FoeCurrent);
            ActiveMonster second = first == PlayerCurrent ? FoeCurrent : PlayerCurrent;

            first.Flinched = false;
            second.Flinched = false;
            first.MoveCancelled = false;
            second.MoveCancelled = false;
            if(PlayerCurrent.QueuedMove == null && playerCanAttack)
                PlayerCurrent.MoveOverrideTemporary = null;
            if(FoeCurrent.QueuedMove == null && foeCanAttack)
                FoeCurrent.MoveOverrideTemporary = null;
            if (first.SubstituteHP <= 0)
                first.SubstituteHP = null;
            if (second.SubstituteHP <= 0)
                second.SubstituteHP = null;

            foreach (ActiveMonster current in new ActiveMonster[] { first, second })
            {
                if (current.Flinched)
                {
                    OnSendMessage("{0}{1} flinched!", current.Trainer.MonNamePrefix, current.Monster.Name);
                    current.Flinched = false;
                    continue;
                }

                ActiveMonster opponent = current == first ? second : first;
                if (current.SelectedMove != null)
                {
                    bool battleContinues = ExecuteMove(current, opponent);
                    if (!battleContinues)
                    {
                        //OnSendMessage("Battle ended due to roar or something");
                        return false;
                    }

                    if (current.Monster.CurrentHP == 0)
                    {
                        HandleFainting(current, current == PlayerCurrent, opponent);

                        if (current.Monster == null)
                            return false;
                    }
                    
                    if (opponent.Monster.CurrentHP == 0)
                    {
                        HandleFainting(opponent, opponent == PlayerCurrent, current);

                        if (opponent.Monster == null)
                            return false;

                        continue;
                    }

                    HandleDamageOverTime(current, opponent);

                    if (current.Monster.CurrentHP == 0)
                    {
                        HandleFainting(current, current == PlayerCurrent, opponent);
                        
                        if (current.Monster == null)
                            return false;
                    }
                }
            }

            return true;
        }

        protected ActiveMonster WhoGoesFirst(ActiveMonster one, ActiveMonster two)
        {
            int onePriority = one.MoveIndex >= 0 ? one.SelectedMove.Priority : -10;
            int twoPriority = two.MoveIndex >= 0 ? two.SelectedMove.Priority : -10;

            if (onePriority > twoPriority)
                return one;
            else if (onePriority < twoPriority)
                return two;

            return one.EffectiveStats.Speed > two.EffectiveStats.Speed ? one : one.EffectiveStats.Speed < two.EffectiveStats.Speed ? two : Rng.Next(0, 2) == 0 ? one : two;
        }

        protected void CancelQueuedMove(ActiveMonster current, CancelMoveReason reason)
        {
            if (current.QueuedMove != null)
            {
                LockInEffect enemyLockIn = (LockInEffect)current.QueuedMove.Effects.Where(e => e.Type == MoveEffectType.LockInMove).FirstOrDefault();
                if (enemyLockIn != null && enemyLockIn.IgnoreCancel != null)
                {
                    if (enemyLockIn.IgnoreCancel.Contains(CancelMoveReason.All) || enemyLockIn.IgnoreCancel.Contains(reason))
                        return;
                }

                current.QueuedMove = null;
                current.QueuedMoveDamage = -1;
                current.QueuedMoveLimit = -1;
            }
        }

        protected bool PreMoveChecks(ActiveMonster current, ActiveMonster opponent)
        {
            //handle current pkmn disabled
            if (current.DisabledCount > 0)
            {
                current.DisabledCount--;
                if (current.DisabledCount <= 0)
                {
                    current.DisabledMoveIndex = -1;
                    OnSendMessage("{0}{1}'s disabled no more!", current.Trainer.MonNamePrefix, current.Monster.Name);
                }
                else if (current.SelectedMove == current.Moves[current.DisabledMoveIndex])
                {
                    OnSendMessage("{0}{1}'s {2} is disabled!", current.Trainer.MonNamePrefix, current.Monster.Name, current.SelectedMove.Name.ToUpper());
                    CancelQueuedMove(current, CancelMoveReason.MoveDisabled);
                    return false;
                }
            }

            //handle current pkmn trapped
            if (opponent.QueuedMove != null && opponent.QueuedMove.Effects.Any(e => e.Type == MoveEffectType.CancelEnemyMove))
            {
                CancelQueuedMove(current, CancelMoveReason.Trapped);
                return false;
            }

            if (current.MoveCancelled)
            {
                CancelQueuedMove(current, CancelMoveReason.Trapped);
                return false;
            }

            //handle current pkmn asleep
            if (current.Monster.Status == StatusCondition.Sleep)
            {
                if (current.QueuedMove != null && !current.QueuedMove.Effects.Any(e => e.Type == MoveEffectType.LockInMove))
                    current.QueuedMove = null;
                current.IsSemiInvulnerable = false;

                current.Monster.SleepCounter--;
                if (current.Monster.SleepCounter <= 0)
                {
                    current.Monster.Status = StatusCondition.None;
                    OnSendMessage("{0}{1} woke up!", current.Trainer.MonNamePrefix, current.Monster.Name);
                    return false;
                }
                else
                {
                    OnSendMessage("{0}{1} is fast asleep!", current.Trainer.MonNamePrefix, current.Monster.Name);
                    CancelQueuedMove(current, CancelMoveReason.Asleep);
                    return false;
                }
            }
            //handle current pkmn frozen
            else if (current.Monster.Status == StatusCondition.Freeze)
            {
                if (current.QueuedMove != null && !current.QueuedMove.Effects.Any(e => e.Type == MoveEffectType.LockInMove))
                    current.QueuedMove = null;
                current.IsSemiInvulnerable = false;

                OnSendMessage("{0}{1} is frozen solid!", current.Trainer.MonNamePrefix, current.Monster.Name);
                CancelQueuedMove(current, CancelMoveReason.Frozen);
                return false;
            }
            //handle current pkmn paralyzed
            else if (current.Monster.Status == StatusCondition.Paralysis && Rng.Next(0, 256) < 63)
            {
                OnSendMessage("{0}{1} is fully paralyzed!", current.Trainer.MonNamePrefix, current.Monster.Name);
                CancelQueuedMove(current, CancelMoveReason.FullyParalyzed);
                return false;
            }

            //handle current pkmn confused
            if (current.IsConfused)
            {
                current.ConfusedCount--;
                if (current.ConfusedCount <= 0)
                {
                    OnSendMessage("{0}{1}'s confused no more!", current.Trainer.MonNamePrefix, current.Monster.Name);
                }
                else
                {
                    OnSendMessage("{0}{1} is confused!", current.Trainer.MonNamePrefix, current.Monster.Name);

                    if (Rng.Next(0, 256) < 128)
                    {
                        OnSendMessage("It hurt itself in its confusion!");
                        if (current.QueuedMove != null && !current.QueuedMove.Effects.Any(e => e.Type == MoveEffectType.LockInMove))
                            current.QueuedMove = null;
                        current.IsSemiInvulnerable = false;

                        int confusionDamage = (int)((2m * current.Monster.Level / 5m + 2m) / 50m * current.EffectiveStats.Attack / current.EffectiveStats.Defense * 40m + 2m);
                        OnSendMessage("Did {0} damage to {1}{2}", confusionDamage, current.Trainer.MonNamePrefix, current.Monster.Name);
                        if (current.SubstituteHP > 0)
                        {
                            current.SubstituteHP -= confusionDamage;
                            OnSendMessage("The SUBSTITUTE took damage for {0}{1}!", current.Trainer.MonNamePrefix, current.Monster.Name);
                            if (opponent.SubstituteHP <= 0)
                                OnSendMessage("{0}{1}'s SUBSTITUTE broke!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                        }
                        else
                        {
                            current.Monster.CurrentHP = Math.Max(0, current.Monster.CurrentHP - confusionDamage);
                            current.AccumulatedDamage += confusionDamage;
                        }
                        CancelQueuedMove(current, CancelMoveReason.HurtInConfusion);
                        return false;
                    }
                }
            }

            //check for charging or recharging
            MultiEffect beforeEffect = (MultiEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Charge && ((MultiEffect)e).When == When.Before).FirstOrDefault();
            MultiEffect afterEffect = (MultiEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Charge && ((MultiEffect)e).When == When.After).FirstOrDefault();

            if (beforeEffect != null)
            {
                if (current.QueuedMove == null)
                {
                    if (!string.IsNullOrWhiteSpace(beforeEffect.Message))
                        OnSendMessage(beforeEffect.Message, current.Trainer.MonNamePrefix, current.Monster.Name);
                    current.QueuedMove = current.SelectedMove;
                    if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.SemiInvulnerable))
                        current.IsSemiInvulnerable = true;
                    return false;
                }
                else
                {
                    for (int i = 0; i < current.Moves.Length; i++)
                    {
                        if (current.Moves[i] == current.QueuedMove)
                        {
                            current.MoveIndex = i;
                            break;
                        }
                    }
                    current.QueuedMove = null;
                }
            }

            if (current.QueuedMove != null && afterEffect != null)
            {
                if (!string.IsNullOrWhiteSpace(afterEffect.Message))
                    OnSendMessage(afterEffect.Message, current.Trainer.MonNamePrefix, current.Monster.Name);
                current.QueuedMove = null;
                return false;
            }

            if (current.SelectedMove != null && current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.RestoreHealth) && current.Monster.CurrentHP == current.Monster.Stats.HP)
            {
                OnSendMessage("{0}{1} used {2}!", current.Trainer.MonNamePrefix, current.Monster.Name, current.SelectedMove.Name.ToUpper());
                OnSendMessage("But, it failed.");
                current.DeductPP();
                return false;
            }

            return true;
        }

        protected int CalculateDamage(ActiveMonster current, ActiveMonster opponent, bool isCriticalHit)
        {
            decimal typeMultiplier = current.SelectedMove.Type.GetEffectiveness(opponent.Type1, opponent.Type2);

            bool immuneToType = typeMultiplier == 0m;

            CustomDamageEffect customEffect = (CustomDamageEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.CustomDamage).FirstOrDefault();
            if (customEffect != null)
            {
                int customDamage = 0;
                switch (customEffect.Calculation)
                {
                    case "level":
                        customDamage = current.Monster.Level;
                        break;
                    case "constant":
                        customDamage = customEffect.Value;
                        break;
                    case "foe-hp-remaining":
                        customDamage = Math.Max(1, (int)(((decimal)opponent.Monster.CurrentHP) * customEffect.Multiplier));
                        break;
                    case "rng-min-1-max-1.5x-level": //Psywave...
                        customDamage = Rng.Next(1, (int)(1.5m * (decimal)current.Monster.Level) + 1);
                        break;
                    case "last-damage-if-normal-or-fighting": //Counter...
                        customDamage = opponent.SelectedMove.Type == Element.Elements["Normal"] | opponent.SelectedMove.Type == Element.Elements["Fighting"] ? (int)(customEffect.Multiplier * (decimal)LastDamageDealt) : 0;
                        break;
                    case "accumulated-on-lock-end": //Bide...
                        customDamage = current.QueuedMoveLimit == 1 ? (int)(customEffect.Multiplier * (decimal)current.AccumulatedDamage) : 0;
                        break;
                }

                if (typeMultiplier == 0m && !current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreTypeImmunity))
                    customDamage = 0;
                else if (!current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreTypeEffectiveness))
                    customDamage = (int)(typeMultiplier * (decimal)customDamage);

                return customDamage;
            }

            //calculate attack and defense
            int att;
            int def;

            if (isCriticalHit)
            {
                att = current.SelectedMove.Type.Category == ElementCategory.Physical ? current.Stats.Attack : current.Stats.Special;
                def = current.SelectedMove.Type.Category == ElementCategory.Physical ? opponent.Stats.Defense : opponent.Stats.Special;
            }
            else
            {
                att = current.SelectedMove.Type.Category == ElementCategory.Physical ? current.EffectiveStats.Attack : current.EffectiveStats.Special;
                def = current.SelectedMove.Type.Category == ElementCategory.Physical ? opponent.EffectiveStats.Defense * opponent.DefenseMultiplier: opponent.EffectiveStats.Special * opponent.SpecialDefenseMultiplier;
            }

            if (def == 0)
                def = 1;

            decimal STAB = 1m;
            if (current.SelectedMove.Type == current.Type1 || current.SelectedMove.Type == current.Type2)
                STAB = 1.5m;

            decimal criticalMultiplier = isCriticalHit ? 2m : 1m;

            decimal modifier = STAB * typeMultiplier * ((decimal)Rng.Next(217, 256)) / 255m;

            int damage = (int)(((2m * current.Monster.Level * criticalMultiplier / 5m + 2m) / 50m * att / def * current.SelectedMove.Power + 2m) * modifier);
            if (current.SelectedMove.Power == 0)
                damage = 0;
            
            return damage;
        }

        protected bool RequirementSatisfied(ActiveMonster current, ActiveMonster opponent, StatusRequirementEffect eff)
        {
            bool ret = true;

            if ((eff.Who == Who.Both || eff.Who == Who.Self) && current.Monster.Status != eff.Status)
                ret = false;

            if ((eff.Who == Who.Both || eff.Who == Who.Foe) && opponent.Monster.Status != eff.Status)
                ret = false;

            return ret;
        }

        protected bool ExecuteMove(ActiveMonster current, ActiveMonster opponent)
        {
            current.LastMoveUsed = current.SelectedMove;

            if (!PreMoveChecks(current, opponent))
                return true;

            MoveEffect alwaysAvailable = current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.NeverDeductPP).FirstOrDefault();
            if (alwaysAvailable != null && !string.IsNullOrEmpty(alwaysAvailable.Message))
                OnSendMessage(alwaysAvailable.Message, current.Trainer.MonNamePrefix, current.Monster.Name);

            //...used move!
            LockInEffect lockInEffect = (LockInEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.LockInMove).FirstOrDefault();
            if (current.QueuedMove != null && lockInEffect != null && !string.IsNullOrEmpty(lockInEffect.Message))
            {
                if (!current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.CustomDamage && ((CustomDamageEffect)e).Calculation == "accumulated-on-lock-end") || current.QueuedMoveLimit == 1)
                    OnSendMessage(lockInEffect.Message, current.Trainer.MonNamePrefix, current.Monster.Name);
            }
            else
            {
                OnSendMessage("{0}{1} used {2}!", current.Trainer.MonNamePrefix, current.Monster.Name, current.SelectedMove.Name.ToUpper());
                current.DeductPP();
            }

            //no effect
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.None))
            {
                OnSendMessage("No effect.");
                return true;
            }

            //doesn't affect type
            foreach(NoEffectEffect eff in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.NoEffect))
            {
                if (opponent.Type1 == eff.Condition || opponent.Type2 == eff.Condition)
                {
                    OnSendMessage("{0}{1} evaded attack!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                    return true;
                }
            }

            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.Substitute))
            {
                HandleSubstituteEffect(current);
                return true;
            }

            //already seeded
            MoveEffect leechSeed = current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.LeechSeed).FirstOrDefault();
            if (leechSeed != null && opponent.IsSeeded)
            {
                OnSendMessage("{0}{1} evaded attack!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                return true;
            }

            //trapping the enemy
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.CancelEnemyMove))
                CancelQueuedMove(opponent, CancelMoveReason.Trapped);

            //mirror move
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.MirrorMove))
            {
                if (opponent.LastMoveUsed == null || opponent.LastMoveUsed == current.SelectedMove)
                {
                    OnSendMessage("The MIRROR MOVE failed!");
                    return true;
                }
                else
                {
                    current.MoveOverrideTemporary = opponent.LastMoveUsed;
                    return ExecuteMove(current, opponent);
                }
            }

            //metronome
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.Random))
            {
                string[] moves = Move.Moves.Values.Where(m => m != current.SelectedMove && !m.Effects.Any(e => e.Type == MoveEffectType.NeverDeductPP)).Select(m => m.Name).ToArray();
                current.MoveOverrideTemporary = Move.Moves[moves[Rng.Next(0, moves.Length)]];
                return ExecuteMove(current, opponent);
            }

            //reset stages
            foreach (StatStageEffect eff in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.ResetStatStages))
                HandleResetStatStageEffect(current, eff, opponent);

            bool triedStatusEffect = false;

            //reset statuses
            foreach (StatusEffect eff in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.ResetStatus))
            {
                if (!string.IsNullOrEmpty(eff.Message))
                    OnSendMessage(eff.Message);

                triedStatusEffect = true;
                HandleResetStatusEffect(current, eff, opponent);
            }

            //protect stages
            foreach (StatStageEffect eff in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.ProtectStatStages))
            {
                triedStatusEffect = true;
                HandleProtectStatStageEffect(current, eff, opponent);
            }

            //calculate critical hit or not
            int critRatio = (int)(((decimal)current.Monster.Species.BaseStats.Speed) / 2m * ((decimal)current.SelectedMove.CritRatio) * ((decimal)current.EffectiveStats.CritRatio) / 100m);
            bool isCriticalHit = Rng.Next(0, 256) < Math.Min(255, critRatio);

            StatusRequirementEffect req = (StatusRequirementEffect)current.SelectedMove.Effects.Where(e => e is StatusRequirementEffect).FirstOrDefault();

            //calculate move hit or miss
            bool moveHit = Rng.Next(0, 256) < (int)(((decimal)current.SelectedMove.Accuracy) * (decimal)current.EffectiveStats.Accuracy / (decimal)opponent.EffectiveStats.Evade);
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.PerfectAccuracy))
                moveHit = true;
            
            if (opponent.IsSemiInvulnerable && !current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreSemiInvulnerability))
                moveHit = false;
            
            if (current.QueuedMove != null && lockInEffect != null && lockInEffect.ConstantDamage)
                moveHit = true;
            
            if (req != null && !RequirementSatisfied(current, opponent, req))
                moveHit = false;

            if(current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.MustBeFaster) && current.EffectiveStats.Speed <= opponent.EffectiveStats.Speed)
                moveHit = false;

            current.IsSemiInvulnerable = false;

            //force run away
            MoveEffect endWildBattle = current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.EndWildBattle).FirstOrDefault();
            if (moveHit && endWildBattle != null)
            {
                if (IsWildBattle)
                {
                    OnSendMessage(endWildBattle.Message ?? "The battle ended!", current.Trainer.MonNamePrefix, current.Monster.Name, opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                    return false;
                }
                else
                {
                    OnSendMessage("But, it failed!");
                    return true;
                }
            }

            bool immuneToType = current.SelectedMove.Type.GetEffectiveness(opponent.Type1, opponent.Type2) == 0m;
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreTypeImmunity))
                immuneToType = false;

            //handle self stat stage effects
            foreach (StatEffect eff in current.SelectedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => string.IsNullOrWhiteSpace(e.Condition) || e.Condition == "defense-only"))
            {
                if ((moveHit && !immuneToType) || eff.Who == Who.Both || eff.Who == Who.Self)
                    triedStatusEffect = true;

                HandleStatEffect(current, null, eff, moveHit);
            }

            //handle pre-damage-calc effects
            foreach (StatEffect eff in current.SelectedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => e.Temporary))
            {
                if ((moveHit && !immuneToType) || eff.Who == Who.Both || eff.Who == Who.Self)
                    triedStatusEffect = true;

                HandleStatEffect(current, opponent, eff, moveHit);
            }
            
            //handle self status condition effects
            foreach (StatusEffect eff in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Status).Cast<StatusEffect>().Where(e => string.IsNullOrEmpty(((StatusEffect)e).Condition)))
            {
                if ((moveHit && !immuneToType) || eff.Who == Who.Both || eff.Who == Who.Self)
                    triedStatusEffect = true;

                HandleStatusEffect(current, current.SelectedMove, null, eff, moveHit);
            }

            //restore health
            HealthEffect restoreHealth = (HealthEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.RestoreHealth).FirstOrDefault();
            if (restoreHealth != null)
                HandleRestoreHealthEffect(current, restoreHealth);

            ExtraDamageEffect crashEffect = (ExtraDamageEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.MissDamage).FirstOrDefault();

            //handle miss
            if (!moveHit && !(current.SelectedMove.Category == ElementCategory.Status && triedStatusEffect))
            {
                OnSendMessage("{0}{1}'s attack missed!", current.Trainer.MonNamePrefix, current.Monster.Name);
                LastDamageDealt = 0;
               
                if (lockInEffect != null && lockInEffect.IgnoreMissOnLock && current.QueuedMove == null)
                {
                    HandleLockInBeginning(current, lockInEffect, opponent);
                }

                if(lockInEffect != null)
                    current.QueuedMoveLimit--;

                if (lockInEffect != null && current.QueuedMove != null && current.QueuedMoveLimit <= 0)
                    HandleLockInEnding(current, opponent, moveHit);

                if (crashEffect != null)
                    HandleMissDamage(current, crashEffect);

                return true;
            }

            //seeded
            if (leechSeed != null)
            {
                opponent.IsSeeded = true;
                OnSendMessage("{0}{1} was seeded!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                return true;
            }

            //transform, mimic, conversion
            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.Copy))
            {
                foreach (CopyEffect copy in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Copy).Cast<CopyEffect>())
                    HandleCopyEffect(current, copy, opponent);

                return true;
            }

            //handle disable effect
            if (moveHit && current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.Disable))
            {
                triedStatusEffect = true;
                HandleDisableEffect(current, opponent);
            }

            int hitsToTry = 1;

            //handle hit-multiple-times
            MultiEffect multiHit = (MultiEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.MultiHit).FirstOrDefault();
            if (multiHit != null)
            {
                if (multiHit.Min == 2 && multiHit.Max == 5) //special case...also most common
                    hitsToTry = new int[] { 2, 2, 2, 3, 3, 3, 4, 5 }[Rng.Next(0, 8)];
                else
                    hitsToTry = Rng.Next(multiHit.Min, multiHit.Max + 1);
            }

            int hitCount = 0;
            int damage;
            if (lockInEffect != null && lockInEffect.ConstantDamage && current.QueuedMoveDamage > 0)
                damage = current.QueuedMoveDamage;
            else if(current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.OneHitKO))
                damage = immuneToType ? 0 : opponent.Monster.CurrentHP;
            else
                damage = CalculateDamage(current, opponent, isCriticalHit);

            if (damage == 0 && current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.CustomDamage && !(((CustomDamageEffect)e).Calculation == "accumulated-on-lock-end" && (current.QueuedMove == null || current.QueuedMoveLimit > 1))))
            {
                OnSendMessage("{0}{1}'s attack missed!", current.Trainer.MonNamePrefix, current.Monster.Name);
                moveHit = false;
            }

            //hit
            for (int i = 0; i < hitsToTry; i++)
            {
                hitCount++;
                if (opponent.SubstituteHP > 0)
                {
                    opponent.SubstituteHP -= damage;
                    if(damage > 0)
                        OnSendMessage("The SUBSTITUTE took damage for {0}{1}!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                    LastDamageDealt = 0;
                    if (opponent.SubstituteHP <= 0)
                        OnSendMessage("{0}{1}'s SUBSTITUTE broke!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                }
                else
                {
                    opponent.Monster.CurrentHP = Math.Max(0, opponent.Monster.CurrentHP - damage);
                    LastDamageDealt = damage;
                    opponent.AccumulatedDamage += damage;
                }

                if (damage != 0)
                    OnSendMessage("Did {0} damage to {1}{2}", damage, opponent.Trainer.MonNamePrefix, opponent.Monster.Name);

                if(opponent.Monster.CurrentHP == 0 && current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.OneHitKO))
                    OnSendMessage("One-hit KO!");

                //apply foe effects
                if (!immuneToType)
                {
                    foreach (StatEffect eff in current.SelectedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => string.IsNullOrWhiteSpace(e.Condition) || e.Condition == "defense-only"))
                    {
                        HandleStatEffect(null, opponent, eff, moveHit);
                    }

                    foreach (StatusEffect eff in current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Status))
                    {
                        HandleStatusEffect(null, current.SelectedMove, opponent, eff, moveHit);
                    }
                }

                //handle rage building
                if (damage > 0 && opponent.QueuedMove != null)
                {
                    foreach (StatEffect eff in opponent.QueuedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => e.Condition == "on-damaged"))
                        HandleStatEffect(opponent, current, eff, moveHit, false);
                }

                //only display messages on first hit
                if (i == 0 && (damage != 0 || !triedStatusEffect) && !(lockInEffect != null && lockInEffect.ConstantDamage && current.QueuedMoveLimit > 0))
                {
                    if (damage != 0 && isCriticalHit)
                        OnSendMessage("Critical hit!");

                    decimal typeMultiplier = current.SelectedMove.Type.GetEffectiveness(opponent.Type1, opponent.Type2);

                    if (typeMultiplier == 0m && !current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreTypeImmunity))
                        OnSendMessage("It doesn't affect {0}{1}.", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                    else if (typeMultiplier > 1m && !current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreTypeEffectiveness))
                        OnSendMessage("It's super effective!");
                    else if (typeMultiplier < 1m && !current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreTypeEffectiveness))
                        OnSendMessage("It's not very effective.");
                }

                if (opponent.Monster.CurrentHP <= 0)
                    break;
            }

            //crash
            if (current.SelectedMove.Type.GetEffectiveness(opponent.Type1, opponent.Type2) == 0m && crashEffect != null)
                HandleMissDamage(current, crashEffect);

            if(hitsToTry > 1 && hitCount > 0)
                OnSendMessage("Hit {0} time(s)!", hitCount);

            if (moveHit && current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.CancelEnemyMove))
            {
                opponent.MoveCancelled = true;
                if(opponent.QueuedMove != null && opponent.QueuedMove.Effects.Any(e => e.Type == MoveEffectType.Charge && ((MultiEffect)e).When == When.After))
                    opponent.QueuedMove = null;
            }

            HealthEffect transferHealth = (HealthEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.TransferHealth).FirstOrDefault();
            if (transferHealth != null)
                HandleTransferHealthEffect(current, transferHealth, opponent, damage * hitCount);

            ExtraDamageEffect recoilEffect = (ExtraDamageEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.RecoilDamage).FirstOrDefault();
            if (recoilEffect != null && (opponent.SubstituteHP == null || opponent.SubstituteHP > 0)) //no recoil if substitute broke
                HandleRecoilEffect(current, recoilEffect, damage * hitCount);

            PayDayEffect payDay = (PayDayEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.PayDay).FirstOrDefault();
            if (payDay != null)
            {
                RewardMoney += (int)(payDay.Multiplier * current.Monster.Level);
                OnSendMessage("Coins scattered everywhere!");
            }

            //handle defrosting
            if (moveHit && current.SelectedMove.CanCauseStatus(StatusCondition.Burn, Who.Foe) && opponent.Monster.Status == StatusCondition.Freeze)
            {
                OnSendMessage("Fire defrosted {0}{1}!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                opponent.Monster.Status = StatusCondition.None;
            }

            //reset temporary stat changes
            foreach (StatEffect eff in current.SelectedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => e.Temporary))
            {
                if (eff.Who == Who.Both || eff.Who == Who.Self)
                    current.Recalc(eff.Stat);

                if (eff.Who == Who.Both || eff.Who == Who.Foe)
                    opponent.Recalc(eff.Stat);
            }

            //handle move lock-in
            if (lockInEffect != null)
            {
                if (moveHit && current.QueuedMove == null)
                    HandleLockInBeginning(current, lockInEffect, opponent);

                current.QueuedMoveLimit--;

                if (current.QueuedMoveLimit <= 0)
                    HandleLockInEnding(current, opponent, moveHit);                    
                else if (lockInEffect.ConstantDamage)
                    current.QueuedMoveDamage = damage;
            }
            else if (lockInEffect == null)
                current.QueuedMove = null;

            //handle hyper beam
            if (current.SelectedMove != null && current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.Charge && ((MultiEffect)e).When == When.After) && opponent.Monster.CurrentHP > 0 && (opponent.SubstituteHP == null || opponent.SubstituteHP > 0))
                current.QueuedMove = current.SelectedMove;

            return true;
        }
    }
}
