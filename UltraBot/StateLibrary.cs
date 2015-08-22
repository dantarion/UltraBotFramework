﻿using System;
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
        public override IEnumerator<string> Run(Bot bot)
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
        public override IEnumerator<string> Run(Bot bot)
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
		
		private List<string> Inputs = new List<string>();
		
		public SequenceState(string sequence)
		{
			foreach(string s in sequence.Split('.'))
				Inputs.Add(s);
		}

        public override IEnumerator<string> Run(Bot bot)
		{
            int index = 0;
            bool stopOnBlock = false;
            bool stopOnWhiff = false;
            //Are we at neutral?
            while (!bot.myState.ActiveCancelLists.Contains("GROUND"))
            {
                yield return "Waiting...to be on ground";
            }
            while(true)
            {                
                //WX wait X frames
			    while(Inputs[index][0] == 'W')
			    {
                    uint timer = UInt32.Parse(Inputs[index++].Substring(1));
                    uint i = 0;
                    while (i++ < timer)
                    {
                        yield return String.Format("Waiting {0} Frames",timer);
                    }
                }
                //Stop on block
                if (Inputs[index].Contains('*'))
                    stopOnBlock = true;
                //Stop on whiff
                if (Inputs[index].Contains('-'))
                    stopOnWhiff = true;

                
                if (stopOnBlock && (bot.enemyState.ScriptName.Contains("GUARD")))
                    yield break;
                if(stopOnWhiff && !(64 <= bot.enemyState.ScriptIndex && bot.enemyState.ScriptIndex <= 202))
                    yield break;
    
                bot.pressButton(Inputs[index]);
                if (index > Inputs.Count - 1)
                    yield break;
                yield return "Pressing" + Inputs[index++].ToString();
                
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
        public override IEnumerator<string> Run(Bot bot)
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
