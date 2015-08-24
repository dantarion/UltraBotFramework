using System;
using UltraBot;

public class ERyuBot : Bot
{
    public ERyuBot()
    {
        RegisterState(typeof(ThrowTechState));
        RegisterState(typeof(DefendState));
    }
    public override BotAIState DefaultState()
    {
        return new IdleState();
    }
    public class TestState : BotAIState
    {
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            while(Math.Abs(bot.myState.XDistance) > .5)
            {
                bot.pressButton("6");
                yield return "Getting in range";
            }
            if(!bot.enemyState.ActiveCancelLists.Contains("REVERSAL"))
            {
            //"1LK.W20.*-1LP.W18.1MP.W8.2.1.4LK.W50.6.2.3HP.W60"
            // .6 1LP.W18.*-MP.W8.4.2.6MK.W52.1MP.W8.2.1.4LK.W50.6.2.3HP.W60"
            // .5 "2MP.W24.*-HP.W8.4.2.6MK.W58.1MP.W8.2.1.4LK.W50.6.2.3HP.W60"
            // .5 "2MP.W24.*-HP.W8.4.2.6MK.W58.1MP.W8.2.3.6HP.W10.MPMK.W10.6.W1.6.W16.1MP.W8.2.1.4LK.W50.6.2.3HP"
            //"2MP.W24.*-HP.W8.4.2.6MK.W58.1MP.W8.2.3.6HP.W10.MPMK.W10.6.W1.6.W16.1MP.W8.2.1.4LK.W50.6.2.3HP.W10.MPMK.W10.6.W1.6.W16.2.6.2.6LKMKHK
            //2MP.W24.*-HP.W8.4.2.6MK.W58.1MP.W8.2.3.6HP.W10.MPMK.W10.6.W1.6.W16.1MP.W8.2.1.4LK.W50.6.2.3HP.W10.MPMK.W10.6.W1.6.W22.2.6.2.6LKMKHK
                var substate = new SequenceState("6MP.W5.LP.W1.LP.W1.6.LK.HP");
                while(!substate.isFinished())
                    yield return substate.Process(bot);
            }
            yield break;
        }
    }
}
