using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
	public class IdleState : BotAIState
	{
		public IdleState()
		{
		}
	}
    public class ThrowTechState : BotAIState
    {
        public override void Run(Bot bot)
        {
            bot.pressButton("LPLK");
            bot.popState();
        }
    }
	public class SequenceState : BotAIState
	{
		[Flags]
		public enum SequenceFlags
		{
			STOP_ON_WHIFF,
			STOP_ON_BLOCK
		}
		private int index = 0;
		private List<string> Inputs = new List<string>();
		private uint timer = 0;
		public SequenceState(string sequence)
		{
			foreach(string s in sequence.Split('.'))
				Inputs.Add(s);


		}

		public override void Run(Bot bot)
		{

            
			if(timer > MatchState.getInstance().FrameCounter)
			{
				return;
			}
            Console.WriteLine("{0} ->{1}", MatchState.getInstance().FrameCounter,Inputs[index]);
			if(Inputs[index][0] == 'W')
			{
				timer = UInt32.Parse(Inputs[index++].Substring(1));
                timer += MatchState.getInstance().FrameCounter;
				return;
			}

            bot.pressButton(Inputs[index++]);
            timer = MatchState.getInstance().FrameCounter+1;
            if (index > Inputs.Count - 1)
                bot.changeState(new IdleState());
		}
		
    }
    public class PokeState : BotAIState
    {
        private string _input;
        private float _distance;
        public PokeState(string input, float distance)
        {
            _input = input;
            _distance = distance;
        }

        public override void Run(Bot bot)
        {
            var spacing = Math.Abs(bot.myState.XDistance) - _distance;
            Console.WriteLine(spacing);
            if (Math.Abs(spacing) > .1)
            {
                if (spacing < 0)
                    bot.pressButton(bot.Back());
                else
                    bot.pressButton(bot.Forward());
                return;
            }
            else
                bot.pressButton(_input);




        }
    }
    public class DefendState : BotAIState
    {
        public static BotAIState Trigger(Bot bot)
        {
            if (bot.enemyState.State == FighterState.CharState.Startup || bot.enemyState.State == FighterState.CharState.Active)
                return new DefendState();
            return null;
        }
        public override void Run(Bot bot)
        {
            if (bot.enemyState.State == FighterState.CharState.Startup || bot.enemyState.State == FighterState.CharState.Active)
            {
                if (bot.enemyState.AState != FighterState.AttackState.Throw)
                {
                    bot.pressButton(bot.Back());
                    if (bot.enemyState.AState != FighterState.AttackState.Overhead)
                        bot.pressButton(bot.Down());
                }
                else
                    bot.pressButton(bot.Up());
                Console.WriteLine("{0} {1}", bot.enemyState.ScriptName, bot.enemyState.StateTimer);
            }
            else
            {
                bot.popState();
            }

        }
    }
}
