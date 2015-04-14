using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UltraBot
{
    public class MatchState
    {
        public uint FrameCounter = 0;
        public uint RoundTimer;
        public bool needStateUpdate = true;
        public void Update()
        {
            var tmp = FrameCounter;
            while (tmp == FrameCounter || Util.GetActiveWindowTitle() != "SSFIVAE")
            {
                tmp = Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A0EB0) + 0x28);
                Thread.Sleep(1);
            }
            if (tmp != FrameCounter+1)
                Console.WriteLine("Dropped {0} frames!!!",tmp-FrameCounter);
            FrameCounter = tmp;
            RoundTimer = Util.Memory.ReadInt((int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6932DC) + 0x3CC) + 0xD0);
            //FrameCounter = KenBot.Memory.ReadInt((int)KenBot.Memory.ReadInt((int)KenBot.Memory.ReadInt(0x400000 + 0x923C68) + 0x4E0) + 0x118);

        }
    }
}
