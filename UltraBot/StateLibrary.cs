using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
	public class IdleState : BotAIState
	{
	}
    public class ReturnToNeutralState : BotAIState
    {
        protected override IEnumerator<string> Run(Bot bot)
        {
            //We wait until we can 
            while(true)
            {
                if(bot.myState.ActiveCancelLists.Contains("GROUND"))
                {
                    yield break;
                }
                yield return "Waiting for Neutral";
            }
        }
    }
    public class ThrowTechState : BotAIState
    {
        public ThrowTechState()
		{
		}
        public static BotAIState Trigger(Bot bot)
        {
            if (bot.myState.ScriptName.Contains("THROW") && bot.myState.ScriptName.Contains("DAMAGE"))
                return new ThrowTechState();
            return null;
        }
        protected override IEnumerator<string> Run(Bot bot)
        {
            //We press tech until we are no longer in the throw tech state
            while (bot.myState.ScriptName.Contains("THROW") && bot.myState.ScriptName.Contains("DAMAGE"))
            {
                bot.pressButton("LPLK");
                yield return "Mashing Tech";
            }
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

        protected override IEnumerator<string> Run(Bot bot)
		{
            bool finished = false;
            while(true)
            {
                //Is it time to do the next input?
			    if(timer > MatchState.getInstance().FrameCounter)
			    {
                    //No, we are waiting, W in effect
				    yield return "Waiting...";
			    }
                //WX wait X frames
			    if(Inputs[index][0] == 'W')
			    {
				    timer = UInt32.Parse(Inputs[index++].Substring(1));
                    timer += MatchState.getInstance().FrameCounter;
                    yield return "Waiting " + MatchState.getInstance().FrameCounter.ToString();
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
                    bot.changeState(new ReturnToNeutralState());
                }
            }

		}
		
    }
    
    public class DefendState : BotAIState
    {
        public DefendState(Bot bot)
		{
		}
        public static BotAIState Trigger(Bot bot)
        {
            
             //bot.enemyState.AttackRange*2+System.Math.Abs(bot.enemyState.XVelocity*bot.enemyState.StateTimer)+.5*System.Math.Abs(bot.enemyState.XAcceleration*3)
            if ((bot.enemyState.State == FighterState.CharState.Startup && bot.enemyState.StateTimer < 3) || bot.enemyState.State == FighterState.CharState.Active)
            {
                Console.WriteLine("VELOCITY={0} ACCEL={1} XPOS={2}", bot.enemyState.XVelocity, bot.enemyState.XAcceleration, bot.enemyState.X);
                if (Math.Abs(bot.myState.XDistance) - .15 < bot.enemyState.AttackRange)

                    return new DefendState(bot);
            }
            return null;
        }
        protected override IEnumerator<string> Run(Bot bot)
        {
            while (true)
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
                    yield break;
                }
                yield return "Blocking";
            }

        }
    }
}
