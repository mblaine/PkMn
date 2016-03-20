using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PkMn.Logic;

namespace PkMn
{
    class PkMnInBox
    {
        public int Box;
        public int Level;
        public Species Type;

        public override string ToString()
        {
            return string.Format("Box {0}: {1} L{2}", Box, Type.Name, Level);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
          
        }


        static void GoGoGadget()
        {
            //var x = ElementType.Types;
            //var y = Species.Types;

            List<Species> need = new List<Species>();

            List<PkMnInBox> have = new List<PkMnInBox>();
            
            foreach (string line in File.ReadAllLines(@"C:\Users\Matthew\Desktop\Programming\C#\PkMn\need.txt").Select(l => Regex.Match(l, "^[0-9]+ (.+)$").Groups[1].Value))
            {
                need.Add(Species.Types[line]);
            }


            int currentBox = 0;
            foreach (string line in File.ReadAllLines(@"C:\Users\Matthew\Desktop\Programming\C#\PkMn\have.txt"))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Match m = Regex.Match(line, "^Box ([0-9]+)$");
                if (m.Success)
                    currentBox = int.Parse(m.Groups[1].Value);
                else
                {
                    Match n = Regex.Match(line, "^(.+) ([0-9]+)$");
                    if (!n.Success)
                        throw new Exception();

                    have.Add(new PkMnInBox() { Box = currentBox, Level = int.Parse(n.Groups[2].Value), Type = Species.Types[n.Groups[1].Value] });
                }
            }

            foreach (var pkmn in need)
            {
                var x = Species.Types.Where(t => t.Value.Evolutions.Any(e => e.Name == pkmn.Name)).Select(v => v.Value).ToArray();

                if (x.Length > 0)
                {
                    if (x.Length > 1)
                        throw new Exception();

                    var candidates = have.Where(h => h.Type == x[0]).Select(c => string.Format("L{0} {1} in Box {2}", c.Level, c.Type.Name, c.Box)).ToArray();
                    if (candidates.Length > 0)
                    {
                        Console.WriteLine("{0} ({1}): Have {2}", pkmn.Name, x[0].Evolutions[0].Condition, string.Join("; ", candidates));
                    }
                    else
                    {
                        //Console.WriteLine("{0} ({1}): Have none", pkmn.Name, x[0].Evolutions[0].Condition);
                    }
                }
                //else
                    //Console.WriteLine(pkmn.Name);
            }
        }
    }
}
