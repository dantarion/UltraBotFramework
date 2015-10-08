using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RainbowLib;
using RainbowLib.BAC;
using RainbowLib.BCM;
using System.IO;
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
        [Flags]
        public enum StatusFlags
        {
            CROUCHING = 0x2,
            AIRBORNE = 0x4,
            COUNTERHIT = 0x10
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

        [Flags]
        public enum Input
        {
            UP = 0x1,
            DOWN = 0x2,
            LEFT = 0x4,
            RIGHT = 0x8,
            LP = 0x10,
            MP = 0x20,
            LK = 0x40,
            MK = 0x80,
            HP = 0x400,
            HK = 0x800,
        }
        //RainbowLibPtrs
        public BCMFile bcm;
        public BACFile bac;
        private int bac_off = 0;
        private int bcm_off = 0;
        /// <summary>
        /// This is used to hold a table to translate between animation ticks and frame timings
        /// </summary>
        private Dictionary<float, float> Tick2Frame = new Dictionary<float, float>();

        private int _BaseOffset;

        public CharState State;
        public AttackState AState;
        public float StateTimer;
        public int PlayerIndex;

        public float X;
        public float Y;
        public float XVelocity;
        public float YVelocity;
        public float XAcceleration;
        public float YAcceleration;
        public float XDistance;
        public float YDistance;
        public StatusFlags Flags;

        public int Health;//TODO
        public int Meter;
        public int Revenge;
        //BAC Data
        public int LastScriptIndex = 0;
        public int ScriptIndex = 0;
        public string LastScriptName = "";
        public string ScriptName = "";
        public float ScriptSpeed;
        public float AttackRange = 0;
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


        //BCM Data
        public List<String> ActiveCancelLists = new List<String>();
        public List<Input> InputBuffer = new List<Input>();

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
        //This checks for a sequence of inputs,
        //Like looking for 6.2.3P to look for buffered shoryu
        public int InputBufferMashCheck(int search, Input input, bool all)
        {
            int mash = 0;
            for (int i = 0; i < search; i++)
            {
                var test = InputBuffer[i];
                if (all)
                {
                    if ((test & input) == input) //All of these must be pressed
                        mash++;
                }
                else
                {
                    if ((test & input) > 0) //Some of these
                        mash++;
                }
            }
            return mash;
        }
        public int InputBufferSequenceCheck(int search, params Input[] sequence)
        {
            int j = sequence.Length - 1;
            for (int i = 0; i < search; i++)
            {
                var test = InputBuffer[i];
                if ((test & sequence[j]) == sequence[j])
                    j--;
                if (j < 0)
                    return i;

            }
            return -1;
        }
        public void UpdatePlayerState()
        {
            int off = 0x8;
            if (PlayerIndex == 1)
                off = 0xC;

            _BaseOffset = (int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A7DCC) + off);
            ReadBCMData();
            ReadBACData();

            ReadOtherData();
        }
        public void ReadBCMData()
        {
            int off = 0x8;
            if (PlayerIndex == 1)
                off = 0xC;
            //06A7DF0
            var InputBufferOffset = (int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x6A7DEC) + off);
            int BCM = (int)Util.Memory.ReadInt(InputBufferOffset + 0x8);
            if (BCM != bcm_off && BCM != 0)
            {
                //Gotta load BCM
                var tmpfile = File.Create(System.IO.Path.GetTempPath() + "/tmp.bcm", 0x4000);
                var tmparr = Util.Memory.ReadAOB(BCM, 0x4000);
                tmpfile.Write(tmparr, 0, tmparr.Length);
                tmpfile.Close();
                bcm = BCMFile.FromFilename(System.IO.Path.GetTempPath() + "/tmp.bcm");
                bcm_off = BCM;
            }
            //Not in a match
            if (BCM == 0)
                return;
            var chargeStatus = (int)Util.Memory.ReadInt(InputBufferOffset + 0x1594);
            ActiveCancelLists.Clear();

            var InputBufferStart = (int)Util.Memory.ReadInt(0x400000 + 0x6A7DF0) + 0x48;
            var InputBufferCurrent = (int)Util.Memory.ReadInt(InputBufferStart + 0x400 * 0xC + 4);
            var InputBufferCurrentAlt = (int)Util.Memory.ReadInt(InputBufferStart - 0x1C) % 0x400;
            InputBufferCurrent = InputBufferCurrentAlt;
            var i = 0;
            while (i < 0x10)
            {
                var test = (int)Util.Memory.ReadInt(InputBufferOffset + 0x147C + i++ * 0x10);
                if (test != -1)
                    if (bcm.CancelLists.Count > test)
                        ActiveCancelLists.Add(bcm.CancelLists[test].Name);
                    else
                        ActiveCancelLists.Add(test.ToString());


            }

            InputBuffer.Clear();

            var InputBufferIndex = (int)Util.Memory.ReadInt(InputBufferOffset + 0x1414);
            for (i = 0; i < 0x400; i++)
            {
                var trueIndex = (InputBufferCurrent);
                var tmp = Util.Memory.ReadInt(InputBufferStart + 0xC * trueIndex);
                InputBuffer.Add((Input)tmp);
            }
        }
        private void ReadBACData()
        {
            var BAC = (int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(_BaseOffset + 0xB0) + 0x8);

            if (BAC != bac_off && BAC != 0)
            {
                //Gotta load BCM
                var tmpfile = File.Create(System.IO.Path.GetTempPath() + "/tmp.bac", 0x4000);
                var tmparr = Util.Memory.ReadAOB(BAC, 0x50000);
                tmpfile.Write(tmparr, 0, tmparr.Length);
                tmpfile.Close();
                bac = BACFile.FromFilename(System.IO.Path.GetTempPath() + "/tmp.bac", bcm);
                bac_off = BAC;
            }
            //Not in a match
            if (BAC == 0)
                return;

            var BAC_data = (int)Util.Memory.ReadInt(_BaseOffset + 0xB0);
            var XChange = X;

            X = Util.Memory.ReadFloat(_BaseOffset + 0x16D0);
            Y = Util.Memory.ReadFloat(_BaseOffset + 0x74);
            XChange = XChange - X;
            XVelocity = Util.Memory.ReadFloat(_BaseOffset + 0xe0);
            if (XVelocity == 0 && XChange != 0)
            {
                XVelocity = XChange;
                //Console.WriteLine("Using {0} for XVel due to XChange", XChange);
            }
            YVelocity = Util.Memory.ReadFloat(_BaseOffset + 0xe4);
            XAcceleration = Util.Memory.ReadFloat(_BaseOffset + 0x100);
            YAcceleration = Util.Memory.ReadFloat(_BaseOffset + 0x104);

            Meter = (int)Util.Memory.ReadShort(_BaseOffset + 0x6C3A);
            Revenge = (int)Util.Memory.ReadShort(_BaseOffset + 0x6C4E);


            Flags = (StatusFlags)Util.Memory.ReadInt(_BaseOffset + 0xBC);
            LastScriptIndex = ScriptIndex;

            ScriptIndex = (int)Util.Memory.ReadInt(BAC_data + 0x18);
            LastScriptName = ScriptName;

            ScriptName = bac.Scripts.Where(x => x.Index == ScriptIndex).FirstOrDefault().Name;
            if (ScriptName == "")
                return;
            ScriptTickTotal = Util.Memory.ReadInt(BAC_data + 0x24) / 0x10000;
            ScriptTickHitboxStart = Util.Memory.ReadInt(BAC_data + 0x28) / 0x10000;
            ScriptTickHitboxEnd = Util.Memory.ReadInt(BAC_data + 0x2C) / 0x10000;
            ScriptTickIASA = Util.Memory.ReadInt(BAC_data + 0x30) / 0x10000;
            ScriptTick = Util.Memory.ReadInt(BAC_data + 0x3C) / 0x10000;

            ScriptSpeed = Util.Memory.ReadInt(BAC_data + 0x18 + 0xC0) / 0x10000;


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
        private void ReadOtherData()
        {

            int off = 0x8;
            if (PlayerIndex == 1)
                off = 0xC;

            var ProjectileOffset = (int)Util.Memory.ReadInt((int)Util.Memory.ReadInt(0x400000 + 0x006A7DE8) + off);
            var tmp1 = (int)Util.Memory.ReadInt((int)ProjectileOffset + 0x4);
            var ProjectileCount = (int)Util.Memory.ReadInt((int)ProjectileOffset + 0x8C);
            if (ProjectileCount != 0)
            {
                var ProjectileLeft = Util.Memory.ReadFloat((int)tmp1 + 0x70);
                var ProjectileRight = Util.Memory.ReadFloat((int)tmp1 + 0x70 + 0x10);
                var ProjectileSpeed = Util.Memory.ReadFloat((int)tmp1 + 0x70 + 0x70);
                var right = Math.Abs(ProjectileRight - X);
                var left = Math.Abs(ProjectileLeft - X);
                var max = Math.Max(right, left);

                AttackRange = Math.Max(max + ProjectileSpeed * 10, AttackRange);
                State = CharState.Active;
            }
            for (int i = 0; i <= 5; i++)
            {

                var hitboxPtr = (int)Util.Memory.ReadInt(_BaseOffset + 0x130 + i * 4);
                var count = (int)Util.Memory.ReadInt(hitboxPtr + 0x2C);
                var start = (int)Util.Memory.ReadInt(hitboxPtr + 0x20);
                for (int j = 0; j < count; j++)
                {
                    if (i == 0)
                    {

                    }
                    //ReadBox here.
                }
            }

        }
        public struct Box
        {
            public float x;
            public float y;
            public float width;
            public float height;
        }
        private void ComputeAttackData(int BAC)
        {
            RainbowLib.BAC.Script currentScript = bac.Scripts.Where(x => x.Index == ScriptIndex).FirstOrDefault();
            var ScriptOffset = BAC + Util.Memory.ReadInt((int)((BAC + Util.Memory.ReadInt((int)BAC + 0x14)) + 4 * ScriptIndex));
            var ScriptCommandListCount = Util.Memory.ReadShort((int)ScriptOffset + 0x12);
            var b = (int)ScriptOffset + 0x18;
            var commandStarts = new List<ushort>();
            var speeds = new List<float>();
            AttackRange = 0;

            var FallBackRange = 0.0f;
            var useFallBack = true;
            foreach (HitboxCommand hitboxCommand in currentScript.CommandLists[(int)CommandListType.HITBOX])
            {

                var type = hitboxCommand.Type;

                var hitlevel = hitboxCommand.HitLevel;
                var flags = hitboxCommand.HitFlags;

                var xoff = hitboxCommand.X;
                var yoff = hitboxCommand.Y;

                var sizex = hitboxCommand.Width;
                var sizey = hitboxCommand.Height;
                var range = xoff + sizex * 2;
                if (type == 0)
                {
                    FallBackRange = range;
                    continue;
                }
                var attach = hitboxCommand.AttachPoint;

                if ((attach) != 0)
                {
                    //Console.WriteLine("WARNING ATTACH " + ScriptName);
                }
                else
                    useFallBack = false;

                AttackRange = Math.Max(AttackRange, range);
                try
                {
                    ScriptFrameHitboxStart = Math.Min(ScriptFrameHitboxStart, Tick2Frame[hitboxCommand.StartFrame]);
                    ScriptFrameHitboxEnd = Math.Max(ScriptFrameHitboxEnd, Tick2Frame[hitboxCommand.EndFrame]);
                }
                catch (Exception)
                {

                }
                if (type.HasFlag(HitboxCommand.HitboxType.GRAB) || flags.HasFlag(HitboxCommand.FlagsType.UNBLOCKABLE))
                {
                    if (!ScriptName.Contains("BALCERONA"))
                    {
                        AState = AttackState.Throw;
                        continue;
                    }
                }


                if (hitlevel == HitboxCommand.HitLevelType.MID)
                    AState = AttackState.Mid;
                if (hitlevel == HitboxCommand.HitLevelType.OVERHEAD)
                    AState = AttackState.Overhead;
                if (hitlevel == HitboxCommand.HitLevelType.LOW)
                    AState = AttackState.Low;
                if (hitlevel == HitboxCommand.HitLevelType.UNBLOCKABLE)
                    AState = AttackState.Throw;
            }
            if (useFallBack == true)
                AttackRange = FallBackRange;

        }
        private void ComputeTickstoFrames(int BAC)
        {
            var ScriptOffset = BAC + Util.Memory.ReadInt((int)((BAC + Util.Memory.ReadInt((int)BAC + 0x14)) + 4 * ScriptIndex));
            var ScriptCommandListCount = Util.Memory.ReadShort((int)ScriptOffset + 0x12);
            var b = (int)ScriptOffset + 0x18;
            var commandStarts = new List<ushort>();
            var speeds = new List<float>();
            if (ScriptCommandListCount == 0)
                return;
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
            if (ScriptName == "2LK")
                Console.Write("trap");
            for (int i = 0; i <= ScriptTickTotal + 5; i++)
            {
                for (int j = 0; j < commandStarts.Count; j++)
                {
                    if (i == commandStarts[j])
                        if (speeds[j] != 0)
                            speed = (float)1.0 / speeds[j];
                        else
                            speed = 0;
                }
                Tick2Frame[i] = curTime;
                curTime += speed;
            }

            ScriptFrame = (float)Math.Ceiling(Tick2Frame[ScriptTick % ScriptTickTotal]);
            ScriptFrameHitboxStart = (float)Math.Ceiling(Tick2Frame[ScriptTickHitboxStart % ScriptTickTotal]);
            ScriptFrameHitboxEnd = (float)Math.Ceiling(Tick2Frame[ScriptTickHitboxEnd % ScriptTickTotal]);
            ScriptFrameIASA = (float)Math.Ceiling(Tick2Frame[ScriptTickIASA % ScriptTickTotal]);
            ScriptFrameTotal = (float)Math.Ceiling(Tick2Frame[ScriptTickTotal]);

        }
        private static string ReadNullTerminatedString(uint offset)
        {
            var tmp = Util.Memory.ReadString((int)offset, 128);
            tmp = tmp.Substring(0, tmp.IndexOf('\x00'));
            return tmp;
        }
        private void ReadStringOffsetTable(List<string> list, int baseOffset, int count, int stringRelativeOffset)
        {
            list.Clear();
            var stringOffset = stringRelativeOffset + baseOffset;
            for (int i = 0; i < count; i++)
            {
                var off = Util.Memory.ReadInt(stringOffset + i * 4);
                list.Add(ReadNullTerminatedString((uint)baseOffset + off));
            }
        }
        public override string ToString()
        {
            return String.Format("X:{0:0.00,4} Y:{1:0.00,4} S:{2,-15} F:{3,4} +:{4,4} -:{5,4} IASA:-:{6,4}", X, Y, ScriptName, ScriptFrame, ScriptFrameHitboxStart, ScriptFrameHitboxEnd, ScriptFrameIASA);
        }
    }

}
