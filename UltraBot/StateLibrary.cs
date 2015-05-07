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
    public class ReturnToNeutralState : BotAIState
    {
        public override void Run(Bot bot)
        {
            if(bot.myState.ActiveCancelLists.Contains("GROUND"))
            {
                bot.popState();
            }
        }
    }
    public class ThrowTechState : BotAIState
    {
        public static BotAIState Trigger(Bot bot)
        {
            if (bot.myState.ScriptName.Contains("THROW") && bot.myState.ScriptName.Contains("DAMAGE"))
                return new ThrowTechState();
            return null;
        }
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
            bool finished = false;
            
            //Is it time to do the next input?
			if(timer > MatchState.getInstance().FrameCounter)
			{
                //No, we are waiting, W in effect
				return;
			}
            //WX wait X frames
			if(Inputs[index][0] == 'W')
			{
				timer = UInt32.Parse(Inputs[index++].Substring(1));
                timer += MatchState.getInstance().FrameCounter;
				return;
			}
            //Stop on block
            if (Inputs[index][0] == '*' && (bot.enemyState.ScriptName.Contains("GUARD") || !(128 <= bot.enemyState.ScriptIndex && bot.enemyState.ScriptIndex <= 202)))
                finished = true;
            else
            {


                bot.pressButton(Inputs[index++]);
                timer = MatchState.getInstance().FrameCounter + 1;
                if (index > Inputs.Count - 1)
                    finished = true;
            }
            
            if(finished)
            {
                timer = 0;
                index = 0;
                bot.pushState(new ReturnToNeutralState());
            }

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
