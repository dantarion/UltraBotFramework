using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    /// <summary>
    /// This function contains data about the fight as a whole.
    /// TODO. Get round count. That will let us code the bot to be more meter heavy when the round is coming to a close, or when its for the win.
    /// </summary>
    public class MatchState
    {
        public uint FrameCounter;
        public uint RoundTimer;
        public void Update()
        {
            FrameCounter = Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A0EB0) + 0x28);
            RoundTimer = Util.Memory.ReadInt((int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6932DC) + 0x3CC) + 0xD0);
            //FrameCounter = KenBot.Memory.ReadInt((int)KenBot.Memory.ReadInt((int)KenBot.Memory.ReadInt(0x400000 + 0x923C68) + 0x4E0) + 0x118);

        }
    }
}
