using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        RegisterState(typeof(DefendState));
        changeState(new SequenceState("LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP.LP.MP.HP"));
    }
}
