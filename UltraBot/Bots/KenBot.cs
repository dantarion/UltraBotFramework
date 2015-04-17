using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        changeState(new SequenceState("2HK"));
    }
    public override void StateCheck()
    {
        if (enemyState.State == FighterState.CharState.Startup || enemyState.State == FighterState.CharState.Active)
            pushState(new DefendState());

    }
}
