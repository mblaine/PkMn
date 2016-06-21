using System;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model
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

        public override string ToString()
        {
            switch (LearnBy)
            {
                case LearnBy.Level:
                    return string.Format("Learns {0} at level {1}", Move.Name, Condition);
                default:
                    return string.Format("Learns {0} with {1}{2:00}", Move.Name, LearnBy, Condition);
            }
        }
    }
}
