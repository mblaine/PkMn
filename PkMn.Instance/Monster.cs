using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model;
using PkMn.Model.Enums;

namespace PkMn.Instance
{
    public class Monster
    {
        public readonly Species Species;
        
        public readonly Stats IV;
        public readonly Stats EV;
        public readonly Stats Stats;

        public readonly Move[] Moves;
        public readonly int[] CurrentPP;
        public readonly int[] PPUpsUsed;

        public int Experience;
        public int Level;
        public StatusCondition Status;
        public string Nickname;

        public int CurrentHP;

        public int SleepCounter;

        public string Name
        {
            get { return !string.IsNullOrWhiteSpace(Nickname) ? Nickname : Species.Name.ToUpper(); }
        }

        public int ExpToLevelUp
        {
            get { return Species.ExpRequired[Species.ExpGrowthRate][Level + 1] - Experience; }
        }

        public Monster(string name, int level, Generator generator = Generator.Wild)
        {
            Species = Species.Spp[name];

            IV = new Stats();
            EV = new Stats();

            if (generator == Generator.Trainer)
            {
                IV.Attack = 9;
                IV.Defense = 8;
                IV.Speed = 8;
                IV.Special = 8;
            }
            else
            {
                IV.Attack = Rng.Next(16);
                IV.Defense = Rng.Next(16);
                IV.Speed = Rng.Next(16);
                IV.Special = Rng.Next(16);
            }

            IV.HP = (IV.Attack & 0x1) << 3
                | (IV.Defense & 0x1) << 2
                | (IV.Special & 0x1) << 1
                | (IV.Speed & 0x1);

            Stats = new Stats();

            Experience = Species.ExpRequired[Species.ExpGrowthRate][level];

            RecalcStats();

            CurrentHP = Stats.HP;
            Status = StatusCondition.None;
            SleepCounter = 0;

            Moves = new Move[4];

            Learnset[] learnset = Species.Learnset.Where(l => l.LearnBy == LearnBy.Level).OrderBy(l => l.Condition).ToArray();

            for (int i = 0; i < learnset.Length; i++)
            {
                if (Level < learnset[i].Condition)
                    break;

                if(!Moves.Contains(learnset[i].Move))
                {
                    if (Moves.Any(m => m == null))
                    {
                        for (int j = 0; j < Moves.Length; j++)
                            if (Moves[j] == null)
                            {
                                Moves[j] = learnset[i].Move;
                                break;
                            }
                    }
                    else
                    {
                        Moves[0] = Moves[1];
                        Moves[1] = Moves[2];
                        Moves[2] = Moves[3];
                        Moves[3] = learnset[i].Move;
                    }
                }
            }

            if (generator == Generator.SimulatePlayer)
            {
                if (Moves.Count(m => m != null) < 4 || (Species.Evolutions.Count > 0 ? Rng.Next(0, 100) < 30 : Rng.Next(0, 100) < 60))
                {
                    Move[] machineMoves = Species.Learnset.Where(l => (l.LearnBy == LearnBy.TM || l.LearnBy == LearnBy.HM) && !Moves.Contains(l.Move)).Select(l => l.Move).ToArray();
                    Move[] sameType = machineMoves.Where(m => m.Type == Species.Type1 || m.Type == Species.Type2).ToArray();
                    Move[] differntType = machineMoves.Where(m => m.Type != Species.Type1 && m.Type != Species.Type2).ToArray();

                    Move[] pickFrom = sameType.Length == 0 ? differntType : differntType.Length == 0 ? sameType : Rng.Next(0, 100) < 60 ? sameType : differntType;

                    if (pickFrom.Length > 0)
                    {
                        Move newMove = pickFrom[Rng.Next(0, pickFrom.Length)];

                        int indexToReplace = Moves.Count(m => m != null);
                        if (indexToReplace < 4)
                            Moves[indexToReplace] = newMove;
                        else
                            Moves[Rng.Next(0, 4)] = newMove;
                    }
                }
            }

            CurrentPP = new int[4];
            PPUpsUsed = new int[4];
            
            for (int i = 0; i < CurrentPP.Length; i++)
            {
                CurrentPP[i] = Moves[i] != null ? Moves[i].PP : 0;
                PPUpsUsed[i] = 0;
            }
        }

        public void RecalcStats()
        {
            Level = Species.ExpRequired[Species.ExpGrowthRate].Where(exp => Experience >= exp).Select((exp, level) => level).Max();
            
            Stats.HP = (int)Math.Floor(((Species.BaseStats.HP + IV.HP) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.HP)) / 4m)) * Level / 100m) + Level + 10;

            Stats.Attack = (int)Math.Floor(((Species.BaseStats.Attack + IV.Attack) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Attack)) / 4m)) * Level / 100m) + 5;
            Stats.Defense = (int)Math.Floor(((Species.BaseStats.Defense + IV.Defense) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Defense)) / 4m)) * Level / 100m) + 5;
            Stats.Special = (int)Math.Floor(((Species.BaseStats.Special + IV.Special) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Special)) / 4m)) * Level / 100m) + 5;
            Stats.Speed = (int)Math.Floor(((Species.BaseStats.Speed + IV.Speed) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Speed)) / 4m)) * Level / 100m) + 5;
        }

        public int MaxPP(int i)
        {
            if (Moves[i] == null)
                return 0;
            return (int)(((decimal)Moves[i].PP) * (1m + 0.2m * PPUpsUsed[i]));
        }

        public override string ToString()
        {
            string status = "";
            switch (Status)
            {
                case StatusCondition.Burn:
                    status = " BRN";
                    break;
                case StatusCondition.Freeze:
                    status = " FRZ";
                    break;
                case StatusCondition.Paralysis:
                    status = " PAR";
                    break;
                case StatusCondition.Poison:
                case StatusCondition.BadlyPoisoned:
                    status = " PSN";
                    break;
                case StatusCondition.Sleep:
                    status = " SLP";
                    break;
                case StatusCondition.Faint:
                    status = " FNT";
                    break;
            }

            return string.Format(":L{0} {1} ({2} / {3}){4}", Level, Name, CurrentHP, Stats.HP, status);
        }

    }
}
