using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
	public class IdleState : BotAIState
	{
		public IdleState()
		{
		}
	}

	public class SequenceState : BotAIState
	{
		[Flags]
		public enum SequenceFlags
		{
			STOP_ON_WHIFF,
			STOP_ON_BLOCK
		}
		private int index = 0;
		private List<string> Inputs = new List<string>();
		private int timer = 0;
		public SequenceState(string sequence)
		{
			foreach(string s in sequence.Split('.'))
				Inputs.Add(s);
		}

		public override void Run(Bot bot)
		{		
			if(timer > 0)
			{
				timer--;
				return;
			}
			if(Inputs[index][0] == 'W')
			{
				timer = Int32.Parse(Inputs[index++].Substring(1));
				return;
			}

            bot.pressButton(Inputs[index++]);	
		}
		
    }
}
