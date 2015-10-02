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
        RegisterState(DefendState.Trigger);
    }
    public override BotAIState DefaultState()
    {
        return new DanceState(2.0f,20,5,10);
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
