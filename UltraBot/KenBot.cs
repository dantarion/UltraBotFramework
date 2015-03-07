using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    public class KenBot : Bot
    {
        public KenBot(int index) : base(index)
        {
            currentState = new TestState();
            currentState.bot = this;
        }
    }
    public class TestState : BotState
    {
        public override void Run()
        {
            if(bot.enemyState.State == FighterState.CharState.Startup)
            { 
                bot.pressButton(bot.Back());
                Console.WriteLine("{0} {1}", bot.enemyState.ScriptName, bot.enemyState.StateTimer);
            }

        }
    }
}
