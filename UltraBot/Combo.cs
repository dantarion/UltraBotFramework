using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    [Flags]
    public enum ComboType
    {
        GROUND = 1, ANTIAIR = 2, SAFE_JUMP = 4, ANYWHERE = 8, MIDSCREEN = 16, CORNER = 32, ULTRA = 64, DEBUG = 128, STUFF = 256, STAND = 512
    }
    public class Combo
    {
        
        public float Score { get; set; }
        public string TypeString 
        { get { return Type.ToString(); } }
        public ComboType Type { get; set; }
        public int Startup { get; set; }
        public float XMin { get; set; }
        public float XMax { get; set; }
        public float YMin { get; set; }
        public float YMax { get; set; }
        public int EXMeter { get; set; }
        public string Input { get; set; }
        

    }
}
