using System;
using UltraBot;
using System.Linq;
public class ERyuBot : Bot
{
    public ERyuBot()
    {
        RegisterState(typeof(ThrowTechState));
        RegisterState(typeof(DefendState));
    }
    public override BotAIState DefaultState()
    {
        return new TestState();
    }
    public class TestState : BotAIState
    {
        public override System.Collections.Generic.IEnumerator<string> Run(Bot bot)
        {
            var r = new Random();
            var chosenCombo = (from combo in bot.getComboList()
                              where
                                  combo.Score > 0
                              orderby combo.Score descending
                              
                              select combo).Take(5);
            if (chosenCombo.Count() == 0)
                yield break;
            var c = chosenCombo.ElementAt(r.Next(chosenCombo.Count()));
            if (c == null)
                yield break;
            var timer = 0;
            while(Math.Abs(bot.myState.XDistance) > c.XMax)
            {
                if (timer++ > 5)
                    yield break;//Reroll
                bot.pressButton("6");
                yield return "Getting in range";
            }
            if(!bot.enemyState.ActiveCancelLists.Contains("REVERSAL") && !bot.enemyState.ScriptName.Contains("UPWARD"))
            {
                var substate = new SequenceState(c.Input);
                while(!substate.isFinished())
                    yield return substate.Process(bot);
            }
            yield break;
        }
    }
}
