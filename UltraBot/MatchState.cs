using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UltraBot
{
    /// <summary>
    /// This function contains data about the fight as a whole.
    /// TODO. Get round count. That will let us code the bot to be more meter heavy when the round is coming to a close, or when its for the win.
    /// </summary>
    public class MatchState
    {
        private MatchState()
        { }
        private static MatchState _ms;
        public static MatchState getInstance()
        {
            if (_ms == null)
                _ms = new MatchState();
            return _ms;
        }
        public uint FrameCounter;
        public uint RoundTimer;
        public void Update()
        {
            var tmp = Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A7DF0) + 0x28);
            while (tmp == FrameCounter)//We are running too fast, we need to wait for the game to continue
            {
                Thread.Sleep(0);
                tmp = Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A7DF0) + 0x28);
            }
            if (tmp != FrameCounter + 1)//We dropped a frame, maybe AI processing is taking too long, or game itself is skipping frames.
                Console.WriteLine("Dropped {0} frames", tmp - FrameCounter);
            FrameCounter = tmp;
            RoundTimer = Util.Memory.ReadInt((int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6932DC) + 0x3CC) + 0xD0);

        }
    }
}
