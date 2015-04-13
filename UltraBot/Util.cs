using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using MemoryEditor;
using System.Threading;
namespace UltraBot
{
    public class Util
    {
        public static Memory Memory= null;
        public static void Init()
        {
            if (Memory == null)
            {
                Memory = new Memory();
                var success = Memory.OpenProcess("SSFIV");
                Console.WriteLine("Connecting to game.");
                while (success == false)
                {
                    Console.Write(".");
                    Thread.Sleep(1000);
                    success = Memory.OpenProcess("SSFIV");
                }
            }
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
    }
}
