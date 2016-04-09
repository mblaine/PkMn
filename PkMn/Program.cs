using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PkMn.Model;
using PkMn.Instance;
using PkMn.Model.Enums;
using PkMn.Model.Moves;

namespace PkMn
{
    class Program
    {
        private static DateTime? lastMessage = null;
        private static StringBuilder sb = new StringBuilder();

        static Monster[] PlayerStatic()
        {
            return new Monster[] { new Monster("Wartortle", 25)};//, new Monster("Raichu", 36), new Monster("Ivysaur", 29), new Monster("Beedrill", 30), null, null };
        }

        static Monster[] RivalStatic()
        {
            return new Monster[] { new Monster("Charmeleon", 32)};//, new Monster("Hypno", 36), new Monster("Onix", 36), null, null };
        }

        static Monster[] Random()
        {
            Monster[] ret = new Monster[6];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new Monster(Species.Spp.Select(p => p.Value).ToArray()[Rng.Next(1, 151)].Name, 50);
            }

            Log(">>> " + string.Join(", ", ret.Select(r => r.Name)));

            return ret;
        }

        static void Main(string[] args)
        {
            bool random = false;

            Trainer player = new Trainer()
            {
                Name = "Matthew",
                MonNamePrefix = "",
                Party = random ? Random() : PlayerStatic()
            };

            Trainer rival = new Trainer()
            {
                Name = "Gary",
                MonNamePrefix = "Enemy ",
                Party = random ? Random() : RivalStatic()
            };

            //player.Party[0].Stats.Speed = 10;
            //player.Party[0].Moves[3] = Move.Moves["Metronome"];
            player.Party[0].Moves[0] = player.Party[0].Moves[1] = player.Party[0].Moves[2] = player.Party[0].Moves[3] = Move.Moves["Water Gun"];
            rival.Party[0].Moves[0] = rival.Party[0].Moves[1] = rival.Party[0].Moves[2] = rival.Party[0].Moves[3] = Move.Moves["Reflect"];

            //player.Party[0].Moves[0] = Move.Moves["Horn Drill"];
            //player.Party[0].Moves[3] = Move.Moves["Disable"];
            //player.Party[0].Moves[3] = Move.Moves["Softboiled"];
            //player.Party[1].Moves[1] = Move.Moves["Thunder Wave"];
            //rival.Party[0].Moves[0] = Move.Moves["String Shot"];
            //rival.Party[0].Moves[2] = Move.Moves["Disable"];

            Battle battle = new Battle(player, rival, true);
            battle.ChooseNextMon += Battle_ChooseMon;
            battle.SendMessage += Battle_SendMessage;
            battle.ChooseAction += Battle_ChooseAction;
            battle.ChooseMoveToMimic += Battle_ChooseMoveToMimic;

            do
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
            while (battle.Step());

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

        public static BattleAction Battle_ChooseAction(Monster current, Trainer trainer)
        {
            BattleAction ret = new BattleAction();

            ret.Type = BattleActionType.UseMove;
            ret.WhichMove = Rng.Next(0, current.Moves.Count(m => m != null));

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
            if (true && (message.ToLower().Contains("counter") || message.ToLower().Contains("unleashed")))
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


        public static Monster Battle_ChooseMon(Trainer trainer)
        {
            foreach (Monster mon in trainer.Party)
            {
                if (mon != null && mon.CurrentHP > 0 && mon.Status != StatusCondition.Faint)
                    return mon;
            }
            return null;
        }

        public static ConsoleColor ForeColor(Color c)
        {
            switch (c)
            {
                case Color.Brown:
                    return ConsoleColor.DarkGray;
                case Color.Pink:
                    return ConsoleColor.Magenta;
                case Color.Purple:
                    return ConsoleColor.DarkMagenta;
                case Color.Green:
                    return ConsoleColor.DarkGreen;
                case Color.Black:
                    return ConsoleColor.White;
                case Color.Gray:
                    return ConsoleColor.DarkGray;
                default:
                    return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), c.ToString(), true);

            }
        }
    }
}

