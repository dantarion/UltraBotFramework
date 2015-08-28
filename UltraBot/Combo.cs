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
        public string InputString 
        { get
            {
                var s = Input;

                s = s.Replace("MPMK.W10.6.W1.6.W18", "FADC.fdash");
                s = s.Replace("6.2.3HP", "h.dp");
                s = s.Replace("6.2.3MP", "m.dp");
                s = s.Replace("2.3.6LP", "l.hadou");
                s = s.Replace("2.3.6MP", "m.hadou");
                s = s.Replace("2.3.6HP", "h.hadou");
                s = s.Replace("2.1.4LK", "l.tatsu");
                s = s.Replace("2.1.4HK", "h.tatsu");
                s = s.Replace("4.2.6LK", "l.axekick");
                s = s.Replace("4.2.6MK", "m.axekick");
                s = s.Replace("1LP", "cr.lp");
                s = s.Replace("1MP", "cr.mp");
                s = s.Replace("1HP", "cr.hp");
                s = s.Replace("1LK", "cr.lk");
                s = s.Replace("1MK", "cr.mk");
                s = s.Replace("1HK", "cr.hk");
                s = s.Replace("LP", "st.lp");
                s = s.Replace("MP", "st.mp");
                s = s.Replace("HP", "st.hp");
                s = s.Replace("HP", "st.hp");
                return s;
            }
        }
        public string Input { get; set; }
        

    }
}
