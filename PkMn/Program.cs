using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PkMn.Model;

namespace PkMn
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var mon in Species.Spp)
                Console.WriteLine(mon.Value.Name + " " + mon.Value.Learnset.Count);
        }
    }
}
