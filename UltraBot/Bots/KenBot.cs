using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        changeState(new SequenceState("LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP"));
    }
    public override void StateCheck()
    {
        if (enemyState.State == FighterState.CharState.Startup || enemyState.State == FighterState.CharState.Active)
            pushState(new DefendState());

    }
}
