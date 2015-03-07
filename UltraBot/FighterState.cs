using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    public class FighterState
    {
        /// <summary>
        /// This is used to track what state the character is in.
        /// Neutral is supposed to mean any state where the character can block or normal out of
        /// Startup is pre-attack active frames
        /// Active is self-explanatory
        /// Recovery is after the attack is over, but before they can block.
        /// </summary>
        public enum CharState
        {
            Neutral,
            Startup,
            Active,
            Recovery
        }
        /// <summary>
        /// TODO: Make the code differentiate between grabs and unblockables.
        /// Doesn't matter for Kenbot as unblockables are too slow to hit him normally
        /// </summary>
        public enum AttackState
        {
            Throw,
            Mid,
            Low,
            Overhead,
            None
        }

        /// <summary>
        /// This is used to hold a tabe
        /// </summary>
        private Dictionary<float, float> Tick2Frame = new Dictionary<float, float>();
        public CharState State;
        public AttackState AState;
        public float StateTimer;
        public int PlayerIndex;
        public float X;
        public float Y;
        public float XVelocity;//TODO
        public float YVelocity;//TODO
        public float XDistance;
        public float YDistance;
        public uint RawState;
        public int Health;//TODO
        public int Meter;//TODO
        public int Revenge;//TODO

        public uint LastScriptIndex = UInt32.MaxValue;
        public uint ScriptIndex = UInt32.MaxValue;
        public string LastScriptName = "";
        public string ScriptName = "";
        public float ScriptSpeed;
        //These are for animation frame numbers
        private float ScriptTick;
        private float ScriptTickHitboxStart;
        private float ScriptTickHitboxEnd;
        private float ScriptTickIASA;
        private float ScriptTickTotal;


        //These are realtime frame numbers 1/60 sec
        public float ScriptFrame;
        public float ScriptFrameHitboxStart;
        public float ScriptFrameHitboxEnd;
        public float ScriptFrameIASA;
        public float ScriptFrameTotal;

        private static FighterState P1;
        private static FighterState P2;
        public static FighterState getFighter(int index)
        {
            if (P1 == null)
                P1 = new FighterState(0);
            if (P2 == null)
                P2 = new FighterState(1);
            return index == 0 ? P1 : P2;
        }

        private FighterState(int index)
        {
            this.PlayerIndex = index;
        }


        public void UpdatePlayerState()
        {
            int off = 0x8;
            if (PlayerIndex == 1)
                off = 0xC;

            //var staticBase = (int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x688E6C) + off);
            var staticBase = (int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A0E8C) + off);
            var BAC = Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x8);
            //Not in a match
            if (BAC == 0)
                return;



            X = Util.Memory.ReadFloat(staticBase + 0x70);
            Y = Util.Memory.ReadFloat(staticBase + 0x74);
            XVelocity = Util.Memory.ReadFloat(staticBase + 0xe0);
            YVelocity = Util.Memory.ReadFloat(staticBase + 0xe4);
            RawState = Util.Memory.ReadInt(staticBase + 0xBC);
            LastScriptIndex = ScriptIndex;
            ScriptIndex = Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x18);
            ScriptTick = (float)Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x1C) / 0x10000;

            var ScriptNameOffset = BAC + Util.Memory.ReadInt((int)((BAC + Util.Memory.ReadInt((int)BAC + 0x1C)) + 4 * ScriptIndex));
            var tmp = Util.Memory.ReadString((int)ScriptNameOffset, 128);
            tmp = tmp.Substring(0, tmp.IndexOf('\x00'));
            LastScriptName = ScriptName;
            ScriptName = tmp;
            if (ScriptName == "")
                return;
            ScriptTickHitboxStart = (float)Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x28) / 0x10000;
            ScriptTickHitboxEnd = (float)Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x2C) / 0x10000;
            ScriptTickIASA = (float)Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x30) / 0x10000;
            ScriptTickTotal = (float)Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x24) / 0x10000;
            ScriptSpeed = (float)Util.Memory.ReadInt((int)Util.Memory.ReadInt(staticBase + 0xB0) + 0x18 + 0xC0) / 0x10000;
            if (ScriptTickIASA == 0)
                ScriptTickIASA = ScriptTickTotal;
            ComputeTickstoFrames(BAC);
            ComputeAttackData(BAC);
           
            if (ScriptFrameHitboxStart != 0)
            {
                if (ScriptFrame <= ScriptFrameHitboxStart)
                {
                    State = CharState.Startup;
                    StateTimer = ScriptFrameHitboxStart - ScriptFrame;

                }
                else if (ScriptFrame <= ScriptFrameHitboxEnd)
                {
                    State = CharState.Active;
                    StateTimer = ScriptFrameHitboxEnd - ScriptFrame;
                }
                else if (ScriptFrameIASA > 0 && ScriptFrame <= ScriptFrameIASA)
                {
                    State = CharState.Recovery;
                    StateTimer = ScriptFrameIASA - ScriptFrame;
                }
                else if (ScriptFrame <= ScriptFrameTotal)
                {
                    State = CharState.Recovery;
                    StateTimer = ScriptFrameTotal - ScriptFrame;
                }
                else
                {
                    State = CharState.Neutral;
                }
            }
            else
            {
                State = CharState.Neutral;
                StateTimer = -1;
                AState = AttackState.None;
            }




        }
        private void ComputeAttackData(uint BAC)
        {
            var ScriptOffset = BAC + Util.Memory.ReadInt((int)((BAC + Util.Memory.ReadInt((int)BAC + 0x14)) + 4 * ScriptIndex));
            var ScriptCommandListCount = Util.Memory.ReadShort((int)ScriptOffset + 0x12);
            var b = (int)ScriptOffset + 0x18;
            var commandStarts = new List<ushort>();
            var speeds = new List<float>();
            for (int i = 0; i < ScriptCommandListCount; i++)
            {
                var scriptType = Util.Memory.ReadShort((int)b + i * 12);
                if (scriptType != 7)
                    continue;
                var commandCount = Util.Memory.ReadShort((int)b + i * 12 + 2);
                var FrameOffset = Util.Memory.ReadShort((int)b + i * 12 + 4);
                var DataOffset = Util.Memory.ReadShort((int)b + i * 12 + 8);
                for (int j = 0; j < commandCount; j++)
                {
                    var type = Util.Memory.ReadByte((int)b + DataOffset + i * 12 + j * 44+(24+2));
                    var hitlevel = Util.Memory.ReadByte((int)b + DataOffset + i * 12 + j * 44 + (24 + 3));
                    var flags = Util.Memory.ReadByte((int)b + DataOffset + i * 12 + j * 44 + (24 + 4));
                    if (type == 0)
                        continue;
                    ScriptFrameHitboxStart = Math.Min(ScriptFrameHitboxStart, Tick2Frame[Util.Memory.ReadShort((int)b + FrameOffset + i * 12 + j * 4)]);
                    ScriptFrameHitboxEnd = Math.Max(ScriptFrameHitboxEnd, Tick2Frame[Util.Memory.ReadShort((int)b + FrameOffset + i * 12 + j * 4 + 2)]);
                    if (type == 2|| (flags & 4) != 0)
                    {
                        AState = AttackState.Throw;
                        return;
                    }

                    
                    if (hitlevel == 0)
                        AState = AttackState.Mid;
                    if (hitlevel == 1)
                        AState = AttackState.Overhead;
                    if (hitlevel == 2)
                        AState = AttackState.Low;
                    if (hitlevel ==3)
                        AState = AttackState.Throw;
                }
            }
        }
        private void ComputeTickstoFrames(uint BAC)
        {
            var ScriptOffset = BAC + Util.Memory.ReadInt((int)((BAC + Util.Memory.ReadInt((int)BAC + 0x14)) + 4 * ScriptIndex));
            var ScriptCommandListCount = Util.Memory.ReadShort((int)ScriptOffset + 0x12);
            var b = (int)ScriptOffset + 0x18;
            var commandStarts = new List<ushort>();
            var speeds = new List<float>();
            for (int i = 0; i < ScriptCommandListCount; i++)
            {
                var scriptType = Util.Memory.ReadShort((int)b + i * 12);
                if (scriptType != 4)
                    continue;
                var commandCount = Util.Memory.ReadShort((int)b + i * 12 + 2);
                var FrameOffset = Util.Memory.ReadShort((int)b + i * 12 + 4);
                var DataOffset = Util.Memory.ReadShort((int)b + i * 12 + 8);
                for (int j = 0; j < commandCount; j++)
                {
                    commandStarts.Add(Util.Memory.ReadShort((int)b + FrameOffset + i * 12 + j * 4));
                    speeds.Add(Util.Memory.ReadFloat((int)b + DataOffset + i * 12 + j * 4));
                }

            }
            Tick2Frame.Clear();
            float speed = 1;
            float curTime = 0;
            for (int i = 0; i <= ScriptTickTotal + 5; i++)
            {
                for (int j = 0; j < commandStarts.Count; j++)
                {
                    if (i > commandStarts[j])
                        if (speeds[j] != 0)
                            speed = (float)1.0 / speeds[j];
                        else
                            speed = 0;
                }
                Tick2Frame[i] = curTime;
                curTime += speed;
            }
           
            ScriptFrame = (float)Math.Ceiling(Tick2Frame[(float)Math.Ceiling(ScriptTick % ScriptTickTotal)]);
            ScriptFrameHitboxStart = (float)Math.Ceiling(Tick2Frame[ScriptTickHitboxStart % ScriptTickTotal]);
            ScriptFrameHitboxEnd = (float)Math.Ceiling(Tick2Frame[ScriptTickHitboxEnd % ScriptTickTotal]);
            ScriptFrameIASA = (float)Math.Ceiling(Tick2Frame[ScriptTickIASA % ScriptTickTotal]);
            ScriptFrameTotal = (float)Math.Ceiling(Tick2Frame[ScriptTickTotal]);
        }

        public override string ToString()
        {
            return String.Format("X:{0:0.00,4} Y:{1:0.00,4} S:{2,-15} F:{3,4} +:{4,4} -:{5,4} IASA:-:{6,4}", X, Y, ScriptName, ScriptFrame, ScriptFrameHitboxStart, ScriptFrameHitboxEnd, ScriptFrameIASA);
        }
    }
    
}
