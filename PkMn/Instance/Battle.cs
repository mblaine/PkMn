using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;
using PkMn.Model.Enums;
using PkMn.Model.Moves;

namespace PkMn.Instance
{
    public class Battle
    {
        public delegate Monster ChooseMonEventHandler(Trainer trainer);
        public delegate void SendMessageEventHandler(string message);
        public delegate BattleAction ChoseActionEventHandler(Monster current, Trainer trainer);

        protected Trainer Player;
        protected Trainer Foe;

        public ActiveMonster PlayerCurrent { get; protected set; }
        public ActiveMonster FoeCurrent { get; protected set; }
        protected bool IsWildBattle;

        protected int runCount;

        public ChooseMonEventHandler ChooseNextMon;
        public SendMessageEventHandler SendMessage;
        public ChoseActionEventHandler ChooseAction;

        public Battle(Trainer player, Trainer foe, bool isWildBattle)
        {
            Player = player;
            Foe = foe;
            IsWildBattle = isWildBattle;
            runCount = 0;

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
                HandleFainting(PlayerCurrent, true);
                return PlayerCurrent.Monster != null;
            }

            if (FoeCurrent.Monster.CurrentHP == 0)
            {
                HandleFainting(FoeCurrent, false);
                return FoeCurrent.Monster != null;
            }

            BattleAction playerAction = null;

            if (PlayerCurrent.QueuedMove == null)
            {
                playerAction = ChooseAction(PlayerCurrent.Monster, Player);

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
                            if (Rng.Next(0, 255) < F)
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

            if(FoeCurrent.QueuedMove == null)
                FoeCurrent.MoveIndex = Rng.Next(0, FoeCurrent.Monster.Moves.Count(m => m != null) - 1);

            ActiveMonster first = WhoGoesFirst(PlayerCurrent, FoeCurrent);
            ActiveMonster second = first == PlayerCurrent ? FoeCurrent : PlayerCurrent;

            first.Flinched = false;
            second.Flinched = false;

            foreach (ActiveMonster current in new ActiveMonster[] { first, second })
            {
                first.Recalc();
                second.Recalc();

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
                        OnSendMessage("Battle ended due to roar or something");
                        return false;
                    }
                    
                    if (opponent.Monster.CurrentHP == 0)
                    {
                        HandleFainting(opponent, opponent == PlayerCurrent);

                        if (opponent.Monster == null)
                            return false;
                    }

                    HandleDamageOverTime(current, opponent);

                    if (current.Monster.CurrentHP == 0)
                    {
                        HandleFainting(current, current == PlayerCurrent);
                        
                        if (current.Monster == null)
                            return false;
                    }
                }
            }

            return true;
        }

        protected void HandleFainting(ActiveMonster current, bool isPlayer)
        {
            if (current.Monster.CurrentHP == 0)
            {
                OnSendMessage("{0}{1} fainted!", current.Trainer.MonNamePrefix, current.Monster.Name);
                current.Monster.Status = StatusCondition.Faint;
                if (isPlayer)
                    current.Monster = ChooseNextMon(current.Trainer);
                else
                    current.Monster = current.Trainer.Party.Where(m => m != null && m.CurrentHP > 0 && m.Status != StatusCondition.Faint).FirstOrDefault();
                
                current.Reset();
                
                if (current.Monster == null)
                {
                    if (isPlayer)
                        OnSendMessage("{0} is out of usable Pokémon! {0} blacked out!", current.Trainer.Name);
                    else
                        OnSendMessage("{0} defeated {1}!", this.Player.Name, current.Trainer.Name);
                }
                else
                {
                    if (isPlayer)
                        OnSendMessage("Go {0}!", current.Monster.Name);
                    else
                        OnSendMessage("{0} sent out {1}!", current.Trainer.Name, current.Monster.Name);
                    current.Recalc();
                }
            }
        }

        protected ActiveMonster WhoGoesFirst(ActiveMonster one, ActiveMonster two)
        {
            if (one.MoveIndex> 0 && one.SelectedMove == Move.Moves["Quick Attack"] && (two.MoveIndex < 0 || two.SelectedMove != Move.Moves["Quick Attack"]))
                return one;

            if (two.MoveIndex > 0 && two.SelectedMove == Move.Moves["Quick Attack"] && (one.MoveIndex < 0 || one.SelectedMove != Move.Moves["Quick Attack"]))
                return two;

            if (one.MoveIndex > 0 && one.SelectedMove == Move.Moves["Counter"] && (two.MoveIndex < 0 || two.SelectedMove != Move.Moves["Counter"]))
                return two;

            if (two.MoveIndex > 0 && two.SelectedMove == Move.Moves["Counter"] && (one.MoveIndex < 0 || one.SelectedMove != Move.Moves["Counter"]))
                return one;

            return one.EffectiveStats.Speed > two.EffectiveStats.Speed ? one : one.EffectiveStats.Speed < two.EffectiveStats.Speed ? two : Rng.Next(0, 1) == 0 ? one : two;
        }

        protected void HandleDamageOverTime(ActiveMonster current, ActiveMonster opponent)
        {
            //OnSendMessage("Handlin dot for {0}", current.Monster.Name);
        }

        protected bool ExecuteMove(ActiveMonster current, ActiveMonster opponent)
        {
            MultiEffect beforeEffect = (MultiEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Charge && ((MultiEffect)e).When == When.Before).FirstOrDefault();
            MultiEffect afterEffect = (MultiEffect)current.SelectedMove.Effects.Where(e => e.Type == MoveEffectType.Charge && ((MultiEffect)e).When == When.After).FirstOrDefault();

            if (current.QueuedMove == null && beforeEffect != null)
            {
                if (!string.IsNullOrWhiteSpace(beforeEffect.Message))
                    OnSendMessage(beforeEffect.Message, current.Trainer.MonNamePrefix, current.Monster.Name);
                current.QueuedMove = current.SelectedMove;
                if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.SemiInvulnerable))
                    current.IsSemiInvulnerable = true;
                return true;
            }

            if (current.QueuedMove != null && afterEffect != null)
            {
                if (!string.IsNullOrWhiteSpace(afterEffect.Message))
                    OnSendMessage(afterEffect.Message, current.Trainer.MonNamePrefix, current.Monster.Name);
                current.QueuedMove = null;
                return true;
            }

            OnSendMessage("{0}{1} used {2}!", current.Trainer.MonNamePrefix, current.Monster.Name, current.SelectedMove.Name.ToUpper());

            current.IsSemiInvulnerable = false;

            if (current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.LockInMove))
                current.QueuedMove = current.SelectedMove;
            else
                current.QueuedMove = null;

            if (opponent.IsSemiInvulnerable && !current.SelectedMove.Effects.Any(e => e.Type == MoveEffectType.IgnoreSemiInvulnerability))
            {
                OnSendMessage("It missed!");
                return true;
            }

            bool triedStatusEffect = false;
            bool actuallyDidStatusEffect = false;

            foreach (StatEffect eff in current.SelectedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => string.IsNullOrWhiteSpace(e.Condition)))
            {
                triedStatusEffect = true;

                if(HandleStatEffect(current, opponent, eff))
                    actuallyDidStatusEffect = true;
            }

            int att = current.SelectedMove.Type.Category == ElementCategory.Physical ? current.EffectiveStats.Attack : current.EffectiveStats.Special;
            int def = current.SelectedMove.Type.Category == ElementCategory.Physical ? opponent.EffectiveStats.Defense : opponent.EffectiveStats.Special;

            decimal STAB = 1m;
            if (current.SelectedMove.Type == current.Monster.Species.Type1 || current.SelectedMove.Type == current.Monster.Species.Type2)
                STAB = 1.5m;

            decimal effectiveness1 = current.SelectedMove.Type.GetEffectiveness(opponent.Monster.Species.Type1);
            decimal effectiveness2 = 1m;
            if(opponent.Monster.Species.Type2 != null)
                effectiveness2 = current.SelectedMove.Type.GetEffectiveness(opponent.Monster.Species.Type2);

            decimal critical = 1m;

            decimal modifier = STAB * effectiveness1 * effectiveness2 * critical * ((decimal)Rng.Next(85, 100)) / 100m;

            int damage = (int)(((2m * current.Monster.Level + 10m) / 250m * att / def * current.SelectedMove.Power + 2m) * modifier);
            if (current.SelectedMove.Power == 0)
                damage = 0;

            foreach(StatusEffect eff in current.SelectedMove.Effects.Where(e => e is StatusEffect))
            {
                if (Rng.Next(0, 255) < eff.Chance)
                {
                    triedStatusEffect = true;
                    if(eff.Who == Who.Self || eff.Who == Who.Both)
                    {
                        if (eff.Status == StatusCondition.Faint)
                            current.Monster.CurrentHP = 0;
                        else if (eff.Status == StatusCondition.Confusion)
                        {
                            if (current.IsConfused)
                                OnSendMessage("{0}{1} is already confused!", current.Trainer.MonNamePrefix, current.Monster.Name);
                            else
                                OnSendMessage("{0}{1} became confused!", current.Trainer.MonNamePrefix, current.Monster.Name);
                            current.IsConfused = true;
                        }
                        else if (eff.Status == StatusCondition.Flinch)
                        {
                            //flinching itself is not implemented
                        }
                        else if (current.Monster.Status == StatusCondition.None)
                        {
                            current.Monster.Status = eff.Status;
                            switch (eff.Status)
                            {
                                case StatusCondition.Paralysis:
                                    OnSendMessage("{0}{1} was paralyzed!", current.Trainer.MonNamePrefix, current.Monster.Name);
                                    break;
                                case StatusCondition.Sleep:
                                    OnSendMessage("{0}{1} fell asleep!", current.Trainer.MonNamePrefix, current.Monster.Name);
                                    break;
                                default:
                                    OnSendMessage("{0}{1} was {2}ed!", current.Trainer.MonNamePrefix, current.Monster.Name, eff.Status.ToString().ToLower());
                                    break;
                            }

                        }
                        else if(current.SelectedMove.Category == ElementCategory.Status)
                            OnSendMessage("It didn't affect {0}{1}.", current.Trainer.MonNamePrefix, current.Monster.Name);
                    }

                    if (eff.Who == Who.Foe || eff.Who == Who.Both)
                    {
                        if (eff.Status == StatusCondition.Faint)
                            opponent.Monster.CurrentHP = 0;
                        else if (eff.Status == StatusCondition.Confusion)
                        {
                            if (opponent.IsConfused)
                                OnSendMessage("{0}{1} is already confused!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                            else
                                OnSendMessage("{0}{1} became confused!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                            opponent.IsConfused = true;
                        }
                        else if (eff.Status == StatusCondition.Flinch)
                        {
                            opponent.Flinched = true;
                        }
                        else if (opponent.Monster.Status == StatusCondition.None)
                        {
                            opponent.Monster.Status = eff.Status;
                            switch (eff.Status)
                            {
                                case StatusCondition.Paralysis:
                                    OnSendMessage("{0}{1} was paralyzed!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                                    break;
                                case StatusCondition.Sleep:
                                    OnSendMessage("{0}{1} fell asleep!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                                    break;
                                default:
                                    OnSendMessage("{0}{1} was {2}ed!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name, eff.Status.ToString().ToLower());
                                    break;
                            }

                        }
                        else if (current.SelectedMove.Category == ElementCategory.Status)
                            OnSendMessage("It didn't affect {0}{1}.", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                    }
                }
            }

            opponent.Monster.CurrentHP = Math.Max(0, opponent.Monster.CurrentHP - damage);

            if (damage > 0 && opponent.QueuedMove != null)
            {
                foreach (StatEffect eff in opponent.QueuedMove.Effects.Where(e => e is StatEffect).Cast<StatEffect>().Where(e => e.Condition == "on-damaged"))
                {
                    HandleStatEffect(opponent, current, eff, false);
                }
            }

            if (damage != 0 || !triedStatusEffect)
            {
                OnSendMessage("Did {0} damage to {1}{2}", damage, opponent.Trainer.MonNamePrefix, opponent.Monster.Name);

                if (effectiveness1 * effectiveness2 == 0m)
                    OnSendMessage("It doesn't effect {0}{1}.", opponent.Trainer.MonNamePrefix, opponent.Monster.Name);
                else if (effectiveness1 * effectiveness2 > 1m)
                    OnSendMessage("It's super effective!");
                else if (effectiveness1 * effectiveness2 < 1m)
                    OnSendMessage("It's not very effective.");
            }
            else if (triedStatusEffect && !actuallyDidStatusEffect)
            {
                OnSendMessage("It failed.");
            }

            if (afterEffect != null)
            {
                current.QueuedMove = current.SelectedMove;
            }

            current.Recalc();
            opponent.Recalc();

            return true;
        }

        protected bool HandleStatEffect(ActiveMonster current, ActiveMonster opponent, StatEffect eff, bool showFailMessage = true)
        {
            if (Rng.Next(0, 255) < eff.Chance)
            {
                if (eff.Who == Who.Self || eff.Who == Who.Both)
                {
                    if (!string.IsNullOrWhiteSpace(eff.Message))
                        OnSendMessage(eff.Message, current.Trainer.MonNamePrefix, current.Monster.Name);

                    if (eff.Temporary)
                    {
                        current.EffectiveStats[eff.Stat] = (int)(((decimal)current.EffectiveStats[eff.Stat]) * eff.Multiplier);
                    }
                    else
                    {
                        if ((eff.Change > 0 && current.StatStages[eff.Stat] >= 6) || (eff.Change < 0 && current.StatStages[eff.Stat] <= 6))
                        {
                            if(showFailMessage)
                                OnSendMessage("It didn't have any effect.");
                        }
                        else
                        {
                            current.StatStages[eff.Stat] += eff.Change;
                            OnSendMessage("{0}{1}'s {2} {3}{4}!", current.Trainer.MonNamePrefix, current.Monster.Name, eff.Stat.ToString().ToLower(), eff.Change > 1 ? "greatly " : eff.Change < -1 ? "sharply " : "", eff.Change > 0 ? "rose" : "fell");
                            current.Recalc();
                        }
                    }
                }

                if (eff.Who == Who.Foe || eff.Who == Who.Both)
                {
                    if (eff.Temporary)
                    {
                        opponent.EffectiveStats[eff.Stat] = (int)(((decimal)opponent.EffectiveStats[eff.Stat]) * eff.Multiplier);
                    }
                    else
                    {
                        if ((eff.Change > 0 && opponent.StatStages[eff.Stat] >= 6) || (eff.Change < 0 && opponent.StatStages[eff.Stat] <= -6))
                        {
                            if(showFailMessage)
                                OnSendMessage("It didn't have any effect.");
                        }
                        else
                        {
                            opponent.StatStages[eff.Stat] += eff.Change;
                            OnSendMessage("{0}{1}'s {2} {3}{4}!", opponent.Trainer.MonNamePrefix, opponent.Monster.Name, eff.Stat.ToString().ToLower(), eff.Change > 1 ? "greatly " : eff.Change < -1 ? "sharply " : "", eff.Change > 0 ? "rose" : "fell");
                            opponent.Recalc();
                        }
                    }
                }

                return true;
            }

            return false;
        }

    }
}
