using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Logic.Enums;

namespace PkMn.Logic
{
    public class Learnset
    {
        public Move Move;
        public LearnBy LearnBy;
        public int Condition;

        public Learnset(XmlNode node)
        {
            Move = Move.Moves[node.Attributes["name"].Value];
            LearnBy = (LearnBy)Enum.Parse(typeof(LearnBy), node.Attributes["by"].Value, true);
            Condition = int.Parse(node.Attributes["condition"].Value);
        }
    }
}
