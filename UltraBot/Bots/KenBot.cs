using System;
using UltraBot;
using UltraBot.StateLibrary;

public class KenBot : Bot
{
    public override string ToString()
    {
        return "KenBot 1.0";
    }
    public KenBot()
    {
        RegisterState(ThrowTechState.Trigger);
        RegisterState(ReversalDP);
        RegisterState(CounterDP);
        RegisterState(DefendState.Trigger);
    }
    public BotAIState ReversalDP(Bot bot)
    {

        //bot.enemyState.AttackRange*2+System.Math.Abs(bot.enemyState.XVelocity*bot.enemyState.StateTimer)+.5*System.Math.Abs(bot.enemyState.XAcceleration*3)
        if (//bot.enemyState.State == FighterState.CharState.Startup
            bot.myState.ActiveCancelLists.Contains("REVERSAL")
            && bot.myState.InputBufferSequenceCheck(10,FighterState.Input.RIGHT,FighterState.Input.DOWN) != -1)
        {
            return new SequenceState("6HP");
        }
        return null;
    }
    public BotAIState CounterDP(Bot bot)
    {

        //bot.enemyState.AttackRange*2+System.Math.Abs(bot.enemyState.XVelocity*bot.enemyState.StateTimer)+.5*System.Math.Abs(bot.enemyState.XAcceleration*3)
        if (bot.enemyState.State == FighterState.CharState.Startup)
        {
            return new SequenceState("6.2.6HP");
        }
        return null;
    }
    
    public override BotAIState DefaultState()
    {
       // return new DanceState(2.0f,20,5,10);
        return new IdleState();
    }
    public class PsychicDPState : BotAIState
    {
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            var alternate = 0;

            while(bot.enemyState.ActiveCancelLists.Contains("GROUND"))
            {
                if(alternate++ % 2 == 0)
                    bot.pressButton("2");
                else
                    bot.pressButton("3");
                yield return "mashing DP motion";
            }
            bot.pressButton("3HP");
            yield return "they pressed a button! doing DP!";
        }
    }
    
    public class TestState : BotAIState
    {
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            while(Math.Abs(bot.myState.XDistance) > 1)
            {
                bot.pressButton("6");
                yield return "Getting in range";
            }
            if(!bot.enemyState.ActiveCancelLists.Contains("REVERSAL"))
            {
                var substate = new SequenceState("1LP.W18.*2HP.2MPHP.W10.6.2.3HP");
                while(!substate.isFinished())
                    yield return substate.Process(bot);
            }
            yield break;
        }
    }
}
