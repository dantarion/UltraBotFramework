using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        changeState(new PokeState("2HK",2));
    }
    public override void StateCheck()
    {
        if (enemyState.State == FighterState.CharState.Startup || enemyState.State == FighterState.CharState.Active)
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
        Console.WriteLine(spacing);
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
            Console.WriteLine("{0} {1}", bot.enemyState.ScriptName, bot.enemyState.StateTimer);
        }
        else
        {
            bot.popState();
        }

    }
}

