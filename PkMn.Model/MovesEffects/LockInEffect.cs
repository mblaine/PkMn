using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
