using System;
using UltraBot;
using System.Linq;
public class ERyuBot : Bot
{
    public ERyuBot()
    {
        RegisterState(ThrowTechState.Trigger);
        RegisterState(DefendState.Trigger);
    }
    public override BotAIState DefaultState()
    {
        return new TestState();
    }
    protected override float scoreCombo(Combo combo, int startup = Int32.MaxValue)
    {
        float score = 1.0f;
        //Killers
            
        if (!combo.Type.HasFlag(ComboType.ANTIAIR) && (enemyState.Y != 0 || enemyState.ScriptName.Contains("2JUMP")))
            return 0;

        if (!combo.Type.HasFlag(ComboType.GROUND) && (enemyState.Y == 0 && !enemyState.ScriptName.Contains("2JUMP")))
            return 0;
        if(myState.Meter < combo.EXMeter)
            return 0;//We don't have the meter
        if(combo.EXMeter > 0)
            score *= (float)myState.Meter / (float)combo.EXMeter;
        if (combo.Type.HasFlag(ComboType.ULTRA) && myState.Revenge < 0x190/2)
            return 0;//We don't have ultra
        else
            score *= (float)myState.Revenge;
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
            score += 100;//We are already in range
        //TODO IF WE ARE ALMOST IN RANGE
        //TODO GROUNDED.ANTIAIR.POKE.THROWF setup
        if (combo.Type.HasFlag(ComboType.DEBUG))
            return float.MaxValue;
        return score;
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
