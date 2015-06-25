using System;
using UltraBot;

public class KenBot : Bot
{
    public KenBot()
    {
        RegisterState(typeof(ThrowTechState));
		RegisterState(typeof(DefendState));
        changeState(new IdleState());
    }
}
