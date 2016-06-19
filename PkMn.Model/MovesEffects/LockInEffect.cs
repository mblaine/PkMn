using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using PkMn.Model.Enums;

namespace PkMn.Model.MoveEffects
{
    public class LockInEffect : MultiEffect
    {
        public readonly bool ConstantDamage;
        public readonly bool IgnoreMissOnLock;
        public readonly CancelMoveReason[] IgnoreCancel;

        protected override string[] ValidAttributes { get { return base.ValidAttributes.Union(new string[] { "constant-damage", "ignore-cancel", "ignore-miss-on-lock" }).ToArray(); } }

        public LockInEffect(MoveEffectType type, XmlNode node)
            : base(type, node)
        {
            ConstantDamage = node.Attributes.Contains("constant-damage") ? bool.Parse(node.Attributes["constant-damage"].Value) : false;
            if (node.Attributes.Contains("ignore-cancel"))
                IgnoreCancel = node.Attributes["ignore-cancel"].Value.Split(',').Select(s => (CancelMoveReason)Enum.Parse(typeof(CancelMoveReason), s, true)).ToArray();
            else
                IgnoreCancel = null;
            IgnoreMissOnLock = node.Attributes.Contains("ignore-miss-on-lock") ? bool.Parse(node.Attributes["ignore-miss-on-lock"].Value) : false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if(Max < 1000)
                sb.AppendFormat("Locks attacker into using only this move for {0} to {1} turns", Min, Max);
            else
                sb.Append("Locks attacker into using only this move until it switches out or faints");

            if (ConstantDamage)
                sb.AppendLine().Append("Deals same amount of damage on subsequent turns as on the first");
            if (IgnoreMissOnLock)
                sb.AppendLine().Append("Lock in even if attack misses");
            if (IgnoreCancel != null && IgnoreCancel.Length > 0)
            {
                if (IgnoreCancel[0] == CancelMoveReason.All)
                    sb.AppendLine().Append("Pauses but does not cancel lock in for any status condition");
                else
                    sb.AppendLine().AppendFormat("Pauses but does not cancel lock in when one of: {0}", string.Join(", ", IgnoreCancel.Select(i => Regex.Replace(i.ToString(), "([a-z])([A-Z])", "$1 $2").ToLower())));
            }

            return sb.ToString();
        }
    }
}
