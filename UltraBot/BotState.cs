using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    public abstract class BotState
    {
        public string Status;
        public Bot bot;
        public virtual void Run()
        {

        }
    }
}
