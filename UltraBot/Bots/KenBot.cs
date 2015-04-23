using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        RegisterState(typeof(DefendState));
        changeState(new SequenceState("1LP.W18.2HP.2MPHP.W10.6.2.3HP.W30.MKMP.W10.6.W2.6.W15.2.6.2.6.LPMPHP"));
    }
}
