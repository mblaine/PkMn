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

        static void Main(string[] args)
        {
            Trainer player = new Trainer()
            {
                Name = "Matthew",
                MonNamePrefix = "",
                Party = new Monster[] { new Monster("Onix", 20) }//, new Monster("Raichu", 36), new Monster("Ivysaur", 29), new Monster("Beedrill", 30), null, null }
            };

            Trainer rival = new Trainer()
            {
                Name = "Gary",
                MonNamePrefix = "Enemy ",
                Party = new Monster[] { new Monster("Cloyster", 34)}//, new Monster("Mewtwo", 20), null, null, null }
            };

            //player.Party[0].Stats.Speed = 10;
            //player.Party[0].Moves[2] = Move.Moves["Rage"];
            //player.Party[0].Moves[3] = Move.Moves["Disable"];
            //player.Party[0].Moves[3] = Move.Moves["Softboiled"];
            //player.Party[1].Moves[1] = Move.Moves["Thunder Wave"];
            //rival.Party[0].Moves[1] = Move.Moves["Thunder Wave"];
            //rival.Party[0].Moves[2] = Move.Moves["Disable"];

            Battle battle = new Battle(player, rival, true);
            battle.ChooseNextMon += Battle_ChooseMon;
            battle.SendMessage += Battle_SendMessage;
            battle.ChooseAction += Battle_ChooseAction;

            do
            {
                Console.WriteLine("------------------------------------------------------------");
                Console.ForegroundColor = ForeColor(battle.FoeCurrent.Monster.Species.DexEntry.Color);
                Console.WriteLine("{0,60}", battle.FoeCurrent.Monster);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("{0,60}", battle.FoeCurrent.Monster.Stats);
                Console.WriteLine("{0,60}", "Stages: " + battle.FoeCurrent.StatStages);
                Console.WriteLine("{0,60}", "Effective: " + battle.FoeCurrent.EffectiveStats);

                Console.ForegroundColor = ForeColor(battle.PlayerCurrent.Monster.Species.DexEntry.Color);
                Console.WriteLine(battle.PlayerCurrent.Monster);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(battle.PlayerCurrent.Monster.Stats);
                Console.WriteLine("Stages: " + battle.PlayerCurrent.StatStages);
                Console.WriteLine("Effective: " + battle.PlayerCurrent.EffectiveStats);

                Console.WriteLine("------------------------------------------------------------");
            }
            while (battle.Step());

            Console.ReadLine();
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
                Console.WriteLine("================================================ {0:mm:ss}", DateTime.Now);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            if (false && (message.ToLower().Contains("was disable") || message.ToLower().Contains("disabled no more")))
                Console.WriteLine(message + " <----------------------------------------------");
            else
                Console.WriteLine(message);

            lastMessage = DateTime.Now;
            
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
                default:
                    return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), c.ToString(), true);

            }
        }
    }
}

