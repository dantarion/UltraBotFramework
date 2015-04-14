using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        currentAIState = new SequenceState("6MK.W30.LPLK");
    }
    public override void StateCheck()
    {
        if (myState.ScriptName == "THROW_F_DAMAGE" || myState.ScriptName == "THROW_B_DAMAGE")
            pushState(new ThrowTechState());
        else if (enemyState.State == FighterState.CharState.Startup || enemyState.State == FighterState.CharState.Active)
            pushState(new DefendState());

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
        }
        else
        {
            bot.popState();
        }

    }
}

