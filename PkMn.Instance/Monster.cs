using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case StatusCondition.Burn:
                        return "BRN";
                    case StatusCondition.Freeze:
                        return "FRZ";
                    case StatusCondition.Paralysis:
                        return "PAR";
                    case StatusCondition.Poison:
                    case StatusCondition.BadlyPoisoned:
                        return "PSN";
                    case StatusCondition.Sleep:
                        return "SLP";
                    case StatusCondition.Faint:
                        return "FNT";
                    default:
                        return "";
                }
            }
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
            Level = Species.ExpRequired[Species.ExpGrowthRate].Where(exp => Experience >= exp).Select((exp, lvl) => lvl).Max();

            RecalcStats();

            CurrentHP = Stats.HP;
            Status = StatusCondition.None;
            SleepCounter = 0;

            Moves = new Move[4];

            GenerateMoves(generator);

            CurrentPP = new int[4];
            PPUpsUsed = new int[4];
            
            for (int i = 0; i < CurrentPP.Length; i++)
            {
                CurrentPP[i] = Moves[i] != null ? Moves[i].PP : 0;
                PPUpsUsed[i] = 0;
            }
        }

        public Monster(XmlNode node, Generator generator = Generator.Wild)
        {
            Species = Species.Spp[node.Attributes["species"].Value];
            
            XmlNode currentStats = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "stats" && n.Attributes.Contains("type") && n.Attributes["type"].Value == "current").FirstOrDefault();
            XmlNode ivs = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "stats" && n.Attributes.Contains("type") && n.Attributes["type"].Value == "iv").FirstOrDefault();
            XmlNode evs = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "stats" && n.Attributes.Contains("type") && n.Attributes["type"].Value == "ev").FirstOrDefault();
            XmlNode moves = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "moves").FirstOrDefault();

            IV = new Stats();

            if (ivs != null)
            {
                IV.HP = int.Parse(ivs.Attributes["hp"].Value);
                IV.Attack = int.Parse(ivs.Attributes["attack"].Value);
                IV.Defense = int.Parse(ivs.Attributes["defense"].Value);
                IV.Special = int.Parse(ivs.Attributes["special"].Value);
                IV.Speed = int.Parse(ivs.Attributes["speed"].Value);
            }
            else
            {
                GenerateIVs(generator);
            }

            EV = new Stats();

            if (evs != null)
            {
                EV.HP = int.Parse(evs.Attributes["hp"].Value);
                EV.Attack = int.Parse(evs.Attributes["attack"].Value);
                EV.Defense = int.Parse(evs.Attributes["defense"].Value);
                EV.Special = int.Parse(evs.Attributes["special"].Value);
                EV.Speed = int.Parse(evs.Attributes["speed"].Value);
            }

            if (node.Attributes.Contains("experience"))
            {
                Experience = int.Parse(node.Attributes["experience"].Value);
                Level = Species.ExpRequired[Species.ExpGrowthRate].Where(exp => Experience >= exp).Select((exp, lvl) => lvl).Max();
            }
            else if (node.Attributes.Contains("level"))
            {
                Level = int.Parse(node.Attributes["level"].Value);
                Experience = Species.ExpRequired[Species.ExpGrowthRate][Level];
            }
            else
            {
                Level = 5;
                Experience = Species.ExpRequired[Species.ExpGrowthRate][Level];
            }

            Stats = new Stats();
            if (currentStats != null)
            {
                Stats.HP = int.Parse(currentStats.Attributes["hp"].Value);
                Stats.Attack = int.Parse(currentStats.Attributes["attack"].Value);
                Stats.Defense = int.Parse(currentStats.Attributes["defense"].Value);
                Stats.Special = int.Parse(currentStats.Attributes["special"].Value);
                Stats.Speed = int.Parse(currentStats.Attributes["speed"].Value);
            }
            else
                RecalcStats();

            CurrentHP = node.Attributes.Contains("current-hp") ? int.Parse(node.Attributes["current-hp"].Value) : Stats.HP;
            Status = node.Attributes.Contains("status") ? (StatusCondition)Enum.Parse(typeof(StatusCondition), node.Attributes["status"].Value, true) : StatusCondition.None;
            SleepCounter = node.Attributes.Contains("sleep-counter") ? int.Parse(node.Attributes["sleep-counter"].Value) : 0;

            Moves = new Move[4];
            CurrentPP = new int[4];
            PPUpsUsed = new int[4];

            if (moves != null && moves.ChildNodes.Cast<XmlNode>().Count(n => n.Name == "move") > 0)
            {
                XmlNode[] moveNodes = moves.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "move").ToArray();

                for (int i = 0; i < moveNodes.Length; i++)
                {
                    Moves[i] = Move.Moves[moveNodes[i].Attributes["name"].Value];

                    PPUpsUsed[i] = moveNodes[i].Attributes.Contains("pp-ups-used") ? int.Parse(moveNodes[i].Attributes["pp-ups-used"].Value) : 0;
                    CurrentPP[i] = moveNodes[i].Attributes.Contains("pp") ? int.Parse(moveNodes[i].Attributes["pp"].Value) : this.MaxPP(i);
                    
                }
            }
            else
            {
                GenerateMoves(generator);

                for (int i = 0; i < CurrentPP.Length; i++)
                {
                    CurrentPP[i] = Moves[i] != null ? Moves[i].PP : 0;
                    PPUpsUsed[i] = 0;
                }
            }

        }

        public void RecalcExperience()
        {
            Experience = Species.ExpRequired[Species.ExpGrowthRate][Level];
        }

        public void RecalcStats(StatType? stat = null)
        {
            if (stat == null || stat == StatType.HP)
                Stats.HP = (int)Math.Floor(((Species.BaseStats.HP + IV.HP) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.HP)) / 4m)) * Level / 100m) + Level + 10;

            if (stat == null || stat == StatType.Attack)
                Stats.Attack = (int)Math.Floor(((Species.BaseStats.Attack + IV.Attack) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Attack)) / 4m)) * Level / 100m) + 5;
            if (stat == null || stat == StatType.Defense)
                Stats.Defense = (int)Math.Floor(((Species.BaseStats.Defense + IV.Defense) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Defense)) / 4m)) * Level / 100m) + 5;
            if (stat == null || stat == StatType.Special)
                Stats.Special = (int)Math.Floor(((Species.BaseStats.Special + IV.Special) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Special)) / 4m)) * Level / 100m) + 5;
            if (stat == null || stat == StatType.Speed)
                Stats.Speed = (int)Math.Floor(((Species.BaseStats.Speed + IV.Speed) * 2m + Math.Floor(Math.Ceiling((decimal)Math.Sqrt(EV.Speed)) / 4m)) * Level / 100m) + 5;
        }

        public void GenerateIVs(Generator generator)
        {
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
        }

        public void GenerateMoves(Generator generator)
        {
            for (int i = 0; i < Moves.Length; i++)
                Moves[i] = null;

            Learnset[] learnset = Species.Learnset.Where(l => l.LearnBy == LearnBy.Level).OrderBy(l => l.Condition).ToArray();

            for (int i = 0; i < learnset.Length; i++)
            {
                if (Level < learnset[i].Condition)
                    break;

                if (!Moves.Contains(learnset[i].Move))
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
        }

        public int MaxPP(int i)
        {
            if (Moves[i] == null)
                return 0;
            return (int)(((decimal)Moves[i].PP) * (1m + 0.2m * PPUpsUsed[i]));
        }

        public override string ToString()
        {
            return string.Format(":L{0} {1} ({2} / {3}){4}", Level, Name, CurrentHP, Stats.HP, " " + StatusText);
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode monster = doc.CreateElement("monster");
            monster.AppendAttribute("species", Species.Name);
            if(!string.IsNullOrWhiteSpace(Nickname))
                monster.AppendAttribute("nickname", Nickname);

            monster.AppendAttribute("level", Level);
            monster.AppendAttribute("experience", Experience);
            monster.AppendAttribute("status", Status.ToString().ToLower());
            monster.AppendAttribute("current-hp", CurrentHP);

            if(SleepCounter > 0)
                monster.AppendAttribute("sleep-counter", SleepCounter);

            foreach (Stats stats in new Stats[] { Stats, IV, EV })
            {
                XmlNode statsNode = doc.CreateElement("stats");
                statsNode.AppendAttribute("type", stats == Stats ? "current" : stats == IV ? "iv" : "ev");
                statsNode.AppendAttribute("hp", stats.HP);
                statsNode.AppendAttribute("attack", stats.Attack);
                statsNode.AppendAttribute("defense", stats.Defense);
                statsNode.AppendAttribute("special", stats.Special);
                statsNode.AppendAttribute("speed", stats.Speed);
                monster.AppendChild(statsNode);
            }

            XmlNode moves = doc.CreateElement("moves");

            for(int i = 0; i < Moves.Length; i++)
            {
                if(Moves[i] == null)
                    continue;
                XmlNode moveNode = doc.CreateElement("move");
                moveNode.AppendAttribute("name", Moves[i].Name);
                if (CurrentPP[i] < Moves[i].PP)
                    moveNode.AppendAttribute("current-pp", CurrentPP[i]);
                if(PPUpsUsed[i] > 0)
                    moveNode.AppendAttribute("pp-ups-used", PPUpsUsed[i]);
                moves.AppendChild(moveNode);
            }

            monster.AppendChild(moves);

            return monster;
        }

    }
}
