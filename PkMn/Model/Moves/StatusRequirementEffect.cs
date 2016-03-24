using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.Moves
{
    public class StatusRequirementEffect : MoveEffect
    {
        public readonly StatusCondition Status;
        public readonly Who Who;

        protected override string[] ValidAttributes { get { return new string[] { "type", "status", "who" }; } }

        public StatusRequirementEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            Status = (StatusCondition)Enum.Parse(typeof(StatusCondition), node.Attributes["status"].Value, true);
            Who = (Who)Enum.Parse(typeof(Who), node.Attributes["who"].Value, true);
        }
    }
}
