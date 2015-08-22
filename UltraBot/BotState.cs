using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{

    public abstract class BotAIState
    {
        public static BotAIState Trigger(Bot bot)
        {
            throw new NotImplementedException("This AI State doesn't support being triggered. Implement Trigger()");
        }
        private IEnumerator<string> _iterator = null;
        private bool _finished = false;
        public virtual string Process(Bot bot)
        {   
            if(_iterator == null)
                _iterator = Run(bot);

            if(_iterator.MoveNext())
                return _iterator.Current;
            else
            {
                _finished = true;
                return this.GetType().Name + " has finished";
            }
        }
        public bool isFinished()
        {
            return _finished;
        }
        public virtual IEnumerator<string> Run(Bot bot)
        {
            while(true)
                yield return this.GetType().Name;
        }
    }
}
