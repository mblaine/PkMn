using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PkMn.Model;
using PkMn.Instance;
using PkMn.Model.Enums;

namespace PkMn
{
    class Program
    {
        static void Main(string[] args)
        {
            Trainer player = new Trainer()
            {
                Name = "Matthew",
                MonNamePrefix = "",
                Party = new Monster[] { new Monster("Charizard", 36), new Monster("Raichu", 36), new Monster("Ivysaur", 29), new Monster("Beedrill", 25), null, null }
            };

            Trainer rival = new Trainer()
            {
                Name = "Gary",
                MonNamePrefix = "Enemy ",
                Party = new Monster[] { new Monster("Blastoise", 36), new Monster("Geodude", 30), new Monster("Mewtwo", 20), null, null, null }
            };

            Battle battle = new Battle(player, rival, true);
            battle.ChooseNextMon += Battle_ChooseMon;
            battle.SendMessage += Battle_SendMessage;
            battle.ChooseAction += Battle_ChooseAction;

            do
            {
                Console.WriteLine("-----------------------------------");
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

                Console.WriteLine("-----------------------------------");
            }
            while (battle.Step());
        }

        public static BattleAction Battle_ChooseAction(Monster current, Trainer trainer)
        {
            BattleAction ret = new BattleAction();

            ret.Type = BattleActionType.UseMove;
            ret.WhichMove = Rng.Next(0, current.Moves.Count(m => m != null) - 1);

            return ret;
        }

        public static void Battle_SendMessage(string message)
        {
            Console.WriteLine(message);
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
                default:
                    return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), c.ToString(), true);

            }
        }
    }
}

