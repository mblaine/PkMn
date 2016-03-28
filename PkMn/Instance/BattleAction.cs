using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkMn.Model.Enums;

namespace PkMn.Instance
{
    public class BattleAction
    {
        public BattleActionType Type;
        public Monster SwitchTo;
        public int WhichMove;

        public BattleAction()
        {
            Type = BattleActionType.None;
            SwitchTo = null;
            WhichMove = -1;
        }

        public BattleAction(BattleActionType type, Monster switchTo = null)
        {
            Type = type;
            SwitchTo = switchTo;
        }
    }
}
