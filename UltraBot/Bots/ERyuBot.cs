using System;
using UltraBot;
using System.Linq;
using UltraBot.StateLibrary;
public class ERyuBot : Bot
{
    public ERyuBot()
    {
        RegisterState(ThrowTechState.Trigger);
		RegisterState(StuffState.Trigger);
        RegisterState(WhiffPunishState.Trigger);
        RegisterState(TriggerDefenseCustom);
    }
    public BotAIState TriggerDefenseCustom(Bot bot)
    {
        
        //bot.enemyState.AttackRange*2+System.Math.Abs(bot.enemyState.XVelocity*bot.enemyState.StateTimer)+.5*System.Math.Abs(bot.enemyState.XAcceleration*3)
        if ((bot.enemyState.State == FighterState.CharState.Startup && bot.enemyState.StateTimer < 1) || bot.enemyState.State == FighterState.CharState.Active)
        {
            //Console.WriteLine("VELOCITY={0} ACCEL={1} XPOS={2}", bot.enemyState.XVelocity, bot.enemyState.XAcceleration, bot.enemyState.X);
            if (Math.Abs(bot.myState.XDistance) - .15 < bot.enemyState.AttackRange)

                return new DefendState(bot);
        }
        return null;
    }
    public override BotAIState DefaultState()
    {
        return new TestState();
    }
    protected override float scoreCombo(Combo combo, int startup = Int32.MaxValue)
    {
        float score = 1.0f;
        //Killers		
        if(myState.Meter < combo.EXMeter)
            return 0;//We don't have the meter
        if(combo.EXMeter > 0)
            score *= (float)myState.Meter / (float)combo.EXMeter;
        if (combo.Type.HasFlag(ComboType.ULTRA))
			if(myState.Revenge < 400/2)
				return 0;//We don't have ultra
			else
			{
				score *= ((float)400)/myState.Revenge;
			}
        //Corner
		
        float cornerDistance;
        if (myState.XDistance > 0)
            cornerDistance = Math.Abs(7.5f + myState.X);//Facing Left
        else
            cornerDistance = Math.Abs(-7.5f + myState.X);//Facing right
        if (combo.Type.HasFlag(ComboType.CORNER) && cornerDistance > 2.5)
            return 0;
        if (combo.Type.HasFlag(ComboType.MIDSCREEN) && cornerDistance < 2.5)
            return 0;
        if (Math.Abs(myState.XDistance) <= combo.XMax && Math.Abs(myState.XDistance) >= combo.XMin)
            score *= 1000;//We are already in range
        //TODO IF WE ARE ALMOST IN RANGE
        //TODO GROUNDED.ANTIAIR.POKE.THROWF setup
		
        if (combo.Type.HasFlag(ComboType.DEBUG))
            return float.MaxValue;
        return score;
    }
    public class StuffState : BotAIState
    {
        public static BotAIState Trigger(Bot bot)
        {

            //bot.enemyState.AttackRange*2+System.Math.Abs(bot.enemyState.XVelocity*bot.enemyState.StateTimer)+.5*System.Math.Abs(bot.enemyState.XAcceleration*3)
            if (bot.enemyState.State == FighterState.CharState.Startup && bot.enemyState.StateTimer > 2 && bot.myState.ActiveCancelLists.Contains("GROUND"))
            {
                Console.WriteLine(bot.enemyState.StateTimer);
                return new StuffState();
            }
            return null;
        }
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            var chosenCombo = (from combo in bot.getComboList()
                               where
                                   combo.Score >= 100 && combo.Type.HasFlag(ComboType.STUFF)
                               orderby combo.Score descending
                               select combo).ToList();
            if (chosenCombo.Count() == 0)
            {
                bot.pressButton("6");
                yield break;
            }
            var c = chosenCombo.First();
            var substate = new SequenceState(c.Input);
            while (!substate.isFinished())
                yield return substate.Process(bot);
        }
    }
    public class WhiffPunishState : BotAIState
    {
        public static BotAIState Trigger(Bot bot)
        {

            //bot.enemyState.AttackRange*2+System.Math.Abs(bot.enemyState.XVelocity*bot.enemyState.StateTimer)+.5*System.Math.Abs(bot.enemyState.XAcceleration*3)
            if (bot.enemyState.State == FighterState.CharState.Recovery && bot.enemyState.StateTimer > 5 && bot.myState.ActiveCancelLists.Contains("GROUND") && bot.enemyState.Y == 0 )
            {
                return new WhiffPunishState();
            }
            return null;
        }
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
			var chosenCombo = (from combo in bot.getComboList()
                              where
                                  combo.Score >= 100
                              orderby combo.Score descending
                              
                              select combo).Take(1);
			var c = chosenCombo.First();
            var substate = new SequenceState(c.Input);
			while(!substate.isFinished())
                yield return substate.Process(bot);
        }
    }
    public class TestState : BotAIState
    {
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            var r = new Random();
            var chosenCombo = (from combo in bot.getComboList()
                              where
                                  combo.Score > 0 && combo.Type.HasFlag(ComboType.GROUND)
                              orderby combo.Score descending
                              
                              select combo).Take(5);
            if (chosenCombo.Count() == 0)
			{
				_reason = "No combos?";
                yield break;
			}
            var c = chosenCombo.ElementAt(r.Next(chosenCombo.Count()));
            var timer = 0;
            while(Math.Abs(bot.myState.XDistance) > c.XMax || bot.enemyState.ActiveCancelLists.Contains("REVERSAL") || bot.enemyState.ScriptName.Contains("UPWARD"))
            {
                bot.pressButton("6");
                if (timer++ > 10)
				{
					_reason = "Rerolling";
                    yield break;
				}
                
                yield return "Getting in range"+timer;
            }

			var substate = new SequenceState(c.Input);
			while(!substate.isFinished())
				yield return substate.Process(bot);
        }
    }
}
