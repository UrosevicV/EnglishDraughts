using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishDraughts.Utils
{
    public class Move
    {
        public int FromI { get; set; }
        public int FromJ { get; set; }
        public int ToI { get; set; }
        public int ToJ { get; set; }

        public bool IsJump { get; set; } = false;

        public Move(int fromI, int fromJ, int toI, int toJ, bool isJump = false)
        {
            FromI = fromI;
            FromJ = fromJ;
            ToI = toI;
            ToJ = toJ;
            IsJump = isJump;
        }

        public override string ToString()
        {
            return $"{(IsJump ? "Jump" : "Move")} from ({FromI},{FromJ}) to ({ToI},{ToJ})";
        }
    }
}

