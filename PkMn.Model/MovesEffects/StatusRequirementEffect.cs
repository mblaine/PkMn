using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class StatusRequirementEffect : MoveEffect
    {
        public readonly StatusCondition Status;
        public readonly Who Who;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] {"status", "who" }).ToArray(); } }

        public StatusRequirementEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Status = (StatusCondition)Enum.Parse(typeof(StatusCondition), node.Attributes["status"].Value, true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
        }

        public override string ToString()
        {
            return string.Format("{0} must be {1} for attack to hit", Who, Status == StatusCondition.Sleep ? "asleep" : Status.ToString().ToLower());
        }
    }
}
