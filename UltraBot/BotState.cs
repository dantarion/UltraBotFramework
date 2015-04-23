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
        public static BotAIState Trigger(Bot bot)
        {
            throw new NotImplementedException("This AI State doesn't support being triggered. Implement Trigger()");
        }
        public virtual void Run(Bot bot)
        {

        }
    }
}
