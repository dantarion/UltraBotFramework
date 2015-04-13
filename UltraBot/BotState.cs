using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    public abstract class BotAIState
    {
        public string Status;
        public virtual void Run(Bot bot)
        {

        }
    }
}
