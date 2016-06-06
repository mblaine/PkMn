using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PkMn.Model;
using PkMn.Instance;
using PkMn.Model.Enums;
using PkMn.Model.MoveEffects;

namespace PkMn
{
    class Program
    {
        private static DateTime? lastMessage = null;
        private static StringBuilder sb = new StringBuilder();

        static void Main(string[] args)
        {
            //var mapping = File.ReadAllLines(@"C:\Users\Matthew\Desktop\pkmn\animations_by_animation.txt").Where(l => l.Length > 0 && l[0] == 'E').ToDictionary(l => l.Split(' ')[1], l => l.Split(' ')[0]);

            //foreach (string line in File.ReadAllLines(@"C:\Users\Matthew\Desktop\pkmn\anim2.txt"))
            //{
            //    //Match m = Regex.Match(line, @"^\tdb (\$[0-9A-F]{2}),(\$[0-9A-F]{2}),(\$[0-9A-F]{2})");
            //    Match m = Regex.Match(line, @"(^ +EF_[^,]+),\$([0-9A-Fa-f]+)$");
            //    if (m.Success)
            //    {
            //        byte b = Convert.ToByte(m.Groups[2].Value, 16);
            //        int sprite = (b >> 6) & 0x3;
            //        int delay = b & 63;
            //        Console.WriteLine("{0},{1},{2}", m.Groups[1].Value, delay, sprite);
            //    }
            //    else
            //        Console.WriteLine(line);
            //}

            //Automatic(false);
            Interactive(args);
        }

        static Monster[] PlayerStatic()
        {
            return new Monster[] { new Monster("Persian", 55)};//, new Monster("Raichu", 36), new Monster("Ivysaur", 29), new Monster("Beedrill", 30), null, null };
        }

        static Monster[] RivalStatic()
        {
            return new Monster[] { new Monster("Gengar", 55)};//, new Monster("Hypno", 36), new Monster("Onix", 36), null, null };
        }

        static Monster[] Random(bool isPlayer)
        {
            Monster[] ret = new Monster[6];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new Monster(Species.Spp.Select(p => p.Value).ToArray()[Rng.Next(1, 151)].Name, 70, isPlayer ? Generator.SimulatePlayer : Generator.Trainer);
            }

            Log(">>> " + string.Join(", ", ret.Select(r => r.Name)));

            return ret;
        }

        static void Interactive(string[] args)
        {
            Trainer player = new Trainer()
            {
                Name = "Matthew",
                MonNamePrefix = "",
                Party = Random(true),
                IsPlayer = true
            };

            Trainer rival = new Trainer()
            {
                Name = "Gary",
                MonNamePrefix = "Enemy ",
                Party = Random(false),
                IsPlayer = false
            };


            //player.Party[0] = new Monster("Mewtwo", 100);
            //rival.Party[0] = new Monster("Gastly", 70);
            //player.Party[0].Moves[0] = Move.Moves["Defense Curl"];

            //player.Party[0] = new Monster("Alakazam", 70);
            //player.Party[0].CurrentHP--;
            //rival.Party[0] = new Monster("Omastar", 70);
            //player.Party[0].Moves[0] = Move.Moves["Self-Destruct"];
            //player.Party[0].CurrentPP[0] = player.Party[0].CurrentPP[1] = player.Party[0].CurrentPP[2] = player.Party[0].CurrentPP[3] = 1;
            //rival.Party[0].Moves[0] = rival.Party[0].Moves[1] = rival.Party[0].Moves[2] = rival.Party[0].Moves[3] = Move.Moves["Wrap"];

            Battle battle = new Battle(player, rival, false, true);
            battle.ChooseMoveToMimic += Battle_ChooseMoveToMimic;

            battle.SendMessage += delegate(string message)
            {
                Console.Write(message);
                Console.ReadLine();
            };

            battle.SendDebugMessage += battle.SendMessage;

            battle.ChooseAction += delegate(ActiveMonster current, Trainer trainer, bool canAttack)
            {
                Console.WriteLine(canAttack);
                string[] moveText = new string[4];
                Move[] playerMoves = battle.PlayerCurrent.Moves;
                for (int i = 0; i < 4; i++)
                {
                    if (playerMoves[i] == null)
                        moveText[i] = "";
                    else
                    {
                        moveText[i] = string.Format("{0} {1,-15}", i + 1, playerMoves[i].Name.ToUpper());
                        if (battle.PlayerCurrent.DisabledMoveIndex == i)
                            moveText[i] += " disabled";
                        else
                            moveText[i] += string.Format(" {0,-2} / {1,-2}", battle.PlayerCurrent.CurrentPP[i], playerMoves[i].PP);
                    }

                }
                
                int moveIndex = 0;
                string s = null;
                BattleAction action = new BattleAction();
                while (true)
                {
                    Log("{0,-30}{1,-30}", moveText[0], moveText[1]);
                    Log("{0,-30}{1,-30}", moveText[2], moveText[3]);
                    Log("------------------------------------------------------------");

                    while ((!int.TryParse(s = Console.ReadLine(), out moveIndex) || moveIndex < 1 || moveIndex > 4) && s.ToLower() != "c") ;

                    if (s.ToLower() == "c")
                    {
                        action.SwitchTo = ChooseMon(trainer.Party, true);
                        if (action.SwitchTo == null)
                            continue;
                        action.Type = BattleActionType.ChangeMon;
                        break;
                    }

                    action.Type = BattleActionType.UseMove;
                    action.WhichMove = moveIndex - 1;
                    break;
                }

                return action;
            };

            battle.ChooseNextMon += delegate(Trainer trainer, bool optional)
            {
                return ChooseMon(trainer.Party, optional);
            };

            while (battle.Step())
            {
                Log("------------------------------------------------------------");
                Console.ForegroundColor = ForeColor(battle.FoeCurrent.Monster.Species.DexEntry.Color);
                Log("{0,60}", battle.FoeCurrent.Monster);
                Console.ForegroundColor = ConsoleColor.Gray;
                Log("{0,60}", battle.FoeCurrent.Stats);
                Log("{0,60}", "Stages: " + battle.FoeCurrent.StatStages);
                Log("{0,60}", "Effective: " + battle.FoeCurrent.EffectiveStats);

                Console.ForegroundColor = ForeColor(battle.PlayerCurrent.Monster.Species.DexEntry.Color);
                Log(battle.PlayerCurrent.Monster.ToString());
                Console.ForegroundColor = ConsoleColor.Gray;
                Log(battle.PlayerCurrent.Stats.ToString());
                Log("Stages: " + battle.PlayerCurrent.StatStages);
                Log("Effective: " + battle.PlayerCurrent.EffectiveStats);
                Log("------------------------------------------------------------");
            }
            

            Log("{0,-10}{1}", rival.Name, string.Join(" ", rival.Party.Select(p => p == null ? "___" : p.Status == StatusCondition.Faint ? "(X)" : p.Status != StatusCondition.None ? "(-)" : "( )")));
            Log("{0,-10}{1}", player.Name, string.Join(" ", player.Party.Select(p => p == null ? "___" : p.Status == StatusCondition.Faint ? "(X)" : p.Status != StatusCondition.None ? "(-)" : "( )")));
            

            Console.ReadLine();
            Console.ReadLine();

        }

        static Monster ChooseMon(Monster[] party, bool canCancel)
        {
            int i = 0;

            foreach (Monster mon in party)
            {
                Console.Write("{0,-3}{1,-30}", i + 1, mon.ToString());
                i++;
                if (i % 2 == 0)
                    Console.WriteLine();
            }

            int choice = 0;

            string s;
            while ((!int.TryParse(s = Console.ReadLine(), out choice) || choice < 1 || choice > party.Length) && !(s == "b" && canCancel)) ;
            Log("------------------------------------------------------------");
            if (s == "b")
                return null;
            return party[choice - 1];
        }

        static void Automatic(bool random)
        {

            Trainer player = new Trainer()
            {
                Name = "Matthew",
                MonNamePrefix = "",
                Party = random ? Random(true) : PlayerStatic(),
                IsPlayer = true
            };

            Trainer rival = new Trainer()
            {
                Name = "Gary",
                MonNamePrefix = "Enemy ",
                Party = random ? Random(false) : RivalStatic(),
                IsPlayer = false
            };

            //player.Party[0].CurrentPP[0] = player.Party[0].CurrentPP[1] = player.Party[0].CurrentPP[2] = player.Party[0].CurrentPP[3] = 1;
            //player.Party[0].Stats.Speed = 200;
            //player.Party[0].CurrentHP = rival.Party[0].CurrentHP = 10000;
            //player.Party[0].Moves[0] = Move.Moves["Transform"];
            //player.Party[0].Moves[1] = Move.Moves["Tackle"];
            //player.Party[0].Moves[0] = player.Party[0].Moves[1] = player.Party[0].Moves[2] = player.Party[0].Moves[3] = Move.Moves["Fire Spin"];
            //rival.Party[0].Moves[0] = rival.Party[0].Moves[1] = rival.Party[0].Moves[2] = rival.Party[0].Moves[3] = Move.Moves["Dig"];

            //player.Party[0].Moves[0] = Move.Moves["Dig"];
            //player.Party[0].Moves[3] = Move.Moves["Disable"];
            //player.Party[0].Moves[3] = Move.Moves["Softboiled"];
            //player.Party[1].Moves[1] = Move.Moves["Thunder Wave"];
            //rival.Party[0].Moves[0] = Move.Moves["String Shot"];
            //rival.Party[0].Moves[2] = Move.Moves["Fly"];
            //rival.Party[0].Moves[3] = Move.Moves["Growl"];

            Battle battle = new Battle(player, rival, false, true);
            battle.ChooseNextMon += Battle_ChooseMon;
            battle.SendMessage += Battle_SendMessage;
            battle.ChooseAction += Battle_ChooseAction;
            battle.ChooseMoveToMimic += Battle_ChooseMoveToMimic;
            //battle.PlayerCurrent.Monster.Status = StatusCondition.BadlyPoisoned;
            //battle.PlayerCurrent.BadlyPoisonedCount = 2;
            //battle.FoeCurrent.Monster.Status = StatusCondition.Freeze;
            //battle.PlayerCurrent.ConfusedCount = 10;

            while (battle.Step())
            {
                Log("------------------------------------------------------------");
                Console.ForegroundColor = ForeColor(battle.FoeCurrent.Monster.Species.DexEntry.Color);
                Log("{0,60}", battle.FoeCurrent.Monster);
                Console.ForegroundColor = ConsoleColor.Gray;
                Log("{0,60}", battle.FoeCurrent.Stats);
                Log("{0,60}", "Stages: " + battle.FoeCurrent.StatStages);
                Log("{0,60}", "Effective: " + battle.FoeCurrent.EffectiveStats);

                Console.ForegroundColor = ForeColor(battle.PlayerCurrent.Monster.Species.DexEntry.Color);
                Log(battle.PlayerCurrent.Monster.ToString());
                Console.ForegroundColor = ConsoleColor.Gray;
                Log(battle.PlayerCurrent.Stats.ToString());
                Log("Stages: " + battle.PlayerCurrent.StatStages);
                Log("Effective: " + battle.PlayerCurrent.EffectiveStats);
                
                string[] moveText = new string[4];
                Move[] playerMoves = battle.PlayerCurrent.Moves;
                for (int i = 0; i < 4; i++)
                {
                    if (playerMoves[i] == null)
                        moveText[i] = "";
                    else
                    {
                        moveText[i] = string.Format("{0,-15}", playerMoves[i].Name.ToUpper());
                        if (battle.PlayerCurrent.DisabledMoveIndex == i)
                            moveText[i] += " disabled";
                        else
                            moveText[i] += string.Format(" {0,-2} / {1,-2}", battle.PlayerCurrent.CurrentPP[i], playerMoves[i].PP);
                    }

                }
                Log("------------------------------------------------------------");
                Log("{0,-30}{1,-30}", moveText[0], moveText[1]);
                Log("{0,-30}{1,-30}", moveText[2], moveText[3]);
                Log("------------------------------------------------------------");
            }

            if (random)
            {
                foreach (Trainer t in new Trainer[] { player, rival })
                {
                    Log(">>> " + string.Join(" ", t.Party.Select(p => p == null ? "___" : p.Status == StatusCondition.Faint ? "(X)" : p.Status != StatusCondition.None ? "(-)" : "( )")));
                }

                File.WriteAllText(Path.Combine(@"C:\Users\Matthew\AppData\Roaming\PkMn\Logs", string.Format("{0:yyyy-MM-dd_HH-mm-ss}.txt", DateTime.Now)), sb.ToString());
            }

            Console.ReadLine();
        }

        public static int Battle_ChooseMoveToMimic(Move[] moves)
        {
            return Rng.Next(0, moves.Count(m => m != null));
        }

        //public static bool firstAttack = true;
        public static BattleAction Battle_ChooseAction(ActiveMonster current, Trainer trainer, bool canAttack)
        {
            BattleAction ret = new BattleAction();

            ret.Type = BattleActionType.UseMove;
            ret.WhichMove = Rng.Next(0, current.Moves.Count(m => m != null));
            //ret.WhichMove = firstAttack ? 0 : 1;
            //firstAttack = false;
            return ret;
        }

        public static void Battle_SendMessage(string message)
        {
            if (lastMessage != null && DateTime.Now - lastMessage >= new TimeSpan(0, 0, 1))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Log("================================================ {0:mm:ss}", DateTime.Now);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            if (true && (message.ToLower().Contains("time(s)!") || message.ToLower().Contains("fury")))
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Log(message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            else
                Log(message);

            lastMessage = DateTime.Now;
            
        }

        private static void Log(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            sb.AppendFormat(format, args);
            sb.AppendLine();
        }

        public static Monster Battle_ChooseMon(Trainer trainer, bool optional)
        {
            if (optional)
                return null;

            foreach (Monster mon in trainer.Party)
            {
                if (mon != null && mon.CurrentHP > 0 && mon.Status != StatusCondition.Faint)
                    return mon;
            }
            return null;
        }

        public static ConsoleColor ForeColor(DexColor c)
        {
            switch (c)
            {
                case DexColor.Brown:
                    return ConsoleColor.DarkGray;
                case DexColor.Pink:
                    return ConsoleColor.Magenta;
                case DexColor.Purple:
                    return ConsoleColor.DarkMagenta;
                case DexColor.Green:
                    return ConsoleColor.DarkGreen;
                case DexColor.Black:
                    return ConsoleColor.White;
                case DexColor.Gray:
                    return ConsoleColor.DarkGray;
                default:
                    return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), c.ToString(), true);

            }
        }
    }
}

