using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    public enum ComboType
    {
        NORMAL
    }
    public class Combo
    {
        public ComboType Type { get; set; }
        public int Startup { get; set; }
        public float XMin { get; set; }
        public float XMax { get; set; }
        public float YMin { get; set; }
        public float YMax { get; set; }
        public int EXMeter { get; set; }
        public bool Ultra { get; set; }
        public string Input { get; set; }
        public int Score { get; set; }
    }
}
