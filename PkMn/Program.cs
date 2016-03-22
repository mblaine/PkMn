using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using PkMn.Logic;

namespace PkMn
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var mov in Move.Moves)
                Console.WriteLine(mov.Value);
        }
    }
}
