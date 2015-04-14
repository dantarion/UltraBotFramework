using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using CSScriptLibrary;
namespace UltraBot
{
    public interface IBot
    {
         void Init(int index);
         void StateCheck();
         void Run();
    }

    public class Bot : IBot
    {
        public static void AddSearchPath(string Dir)
        {
            CSScript.GlobalSettings.AddSearchDir(Dir);
        }
        public static IBot LoadBotFromFile(string BotName)
        {
            
            var tmp = CSScript.Load(BotName + ".cs");
            var tmp2 = tmp.CreateInstance(BotName);
            IBot bot = tmp2.AlignToInterface<IBot>();
            return bot;

        }
        public void Init(int index)
        {
            myState = FighterState.getFighter(index);
            enemyState = FighterState.getFighter(index == 0 ? 1 : 0);
        }
        public FighterState myState;
        public FighterState enemyState;
        public List<BotAIState> stateStack = new List<BotAIState>();
        private List<VirtualKeyCode> pressed = new List<VirtualKeyCode>();
        private List<VirtualKeyCode> last_pressed = new List<VirtualKeyCode>();
        private List<VirtualKeyCode> held = new List<VirtualKeyCode>();
        public BotAIState currentAIState;
        public BotAIState previousState;
        public virtual void StateCheck()
        {

        }

        public virtual void Run()
        {
            //Setup some derived variables.
            myState.XDistance = myState.X - enemyState.X;
            myState.YDistance = myState.Y - enemyState.Y;

            StateCheck();
            currentAIState.Run(this);
            if (Util.GetActiveWindowTitle() == "SSFIVAE")
            {
                foreach (var key in pressed)
                {                                   
                    WindowsInput.InputSimulator.SimulateKeyDown(map(key));
                }
                foreach(var key in last_pressed)
                    if(!pressed.Contains(key))
                        WindowsInput.InputSimulator.SimulateKeyUp(map(key));
                last_pressed.Clear();
                last_pressed.AddRange(pressed);
                pressed.Clear();
            }
        }

private static WindowsInput.VirtualKeyCode map(VirtualKeyCode key)
{
    WindowsInput.VirtualKeyCode rawKey;
    switch(key)
    {
        case VirtualKeyCode.DOWN:
        default:
            rawKey = WindowsInput.VirtualKeyCode.DOWN;
            break;
        case VirtualKeyCode.LEFT:
            rawKey = WindowsInput.VirtualKeyCode.LEFT;
            break;
        case VirtualKeyCode.RIGHT:
            rawKey = WindowsInput.VirtualKeyCode.RIGHT;
            break;
        case VirtualKeyCode.UP:
            rawKey = WindowsInput.VirtualKeyCode.UP;
            break;
        case VirtualKeyCode.LP:
            rawKey = WindowsInput.VirtualKeyCode.VK_9;
            break;
        case VirtualKeyCode.MP:
            rawKey = WindowsInput.VirtualKeyCode.VK_0;
            break;
        case VirtualKeyCode.HP:
            rawKey = WindowsInput.VirtualKeyCode.SUBTRACT;
            break;
        case VirtualKeyCode.PPP:
            rawKey = WindowsInput.VirtualKeyCode.OEM_PLUS;
            break;
        case VirtualKeyCode.LK:
            rawKey = WindowsInput.VirtualKeyCode.VK_O;
            break;
        case VirtualKeyCode.MK:
            rawKey = WindowsInput.VirtualKeyCode.VK_P;
            break;
        case VirtualKeyCode.HK:
            rawKey = WindowsInput.VirtualKeyCode.OEM_4;
            break;
        case VirtualKeyCode.KKK:
            rawKey = WindowsInput.VirtualKeyCode.OEM_6;
            break;

    }
    return rawKey;
}
 
        /// <summary>
        /// This changes state.
        /// </summary>
        /// <param name="nextState"></param>
        public void changeState(BotAIState nextState)
        {
            previousState = currentAIState;
            currentAIState = nextState;
            Console.WriteLine("changing from {0} to {1}", previousState, currentAIState);
        }
        /// <summary>
        /// This can be used to return to a previous state.
        /// </summary>
        public void popState()
        {
            if(stateStack.Count() == 0)
            {
                stateStack.Add(new IdleState());
            }
            changeState(stateStack[0]);
            stateStack.RemoveAt(0);
        }
        /// <summary>
        /// This function can be used to switch to a state
        /// while adding the old state onto the stack. This lets you return to that state by calling popState() from the new state 
        /// </summary>
        /// <param name="nextState"></param>
        public void pushState(BotAIState nextState)
        {
            stateStack.Add(currentAIState);
            changeState(nextState);
        }
                /// <summary>
        /// These keycodes exist so that we can map them to keyboard or vJoy or whatever.
        /// </summary>
        public enum VirtualKeyCode
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            START,
            SELECT,
            LP,
            MP,
            HP,
            PPP,
            LK,
            MK,
            HK,
            KKK,
            THROW,
            FOCUS
        }
        public VirtualKeyCode Forward()
        {
            if (myState.XDistance > 0)
                return VirtualKeyCode.LEFT;
            return VirtualKeyCode.RIGHT;
        }
        public VirtualKeyCode Up()
        {
            return VirtualKeyCode.UP;
        }
        public VirtualKeyCode Down()
        {
            return VirtualKeyCode.DOWN;
        }
        public VirtualKeyCode Back()
        {
            if (myState.XDistance > 0)
                return VirtualKeyCode.RIGHT;
            return VirtualKeyCode.LEFT;
        }
        public void pressButton(string key)
        {
            if (key.Contains("2"))
                pressButton(VirtualKeyCode.DOWN);
            if (key.Contains("6"))
                pressButton(this.Forward());
            if (key.Contains("4"))
                pressButton(this.Back());
            if (key.Contains("8"))
                pressButton(VirtualKeyCode.UP);
			if (key.Contains("1"))	
			{
				pressButton(this.Back());
				pressButton(VirtualKeyCode.DOWN);
			}
			if (key.Contains("1"))	
			{
				pressButton(this.Forward());
				pressButton(VirtualKeyCode.DOWN);
			}
			if (key.Contains("7"))	
			{
				pressButton(this.Back());
				pressButton(VirtualKeyCode.UP);
			}
			if (key.Contains("9"))	
			{
				pressButton(this.Forward());
                pressButton(VirtualKeyCode.UP);
			}
			
            if (key.Contains("LP"))
                pressButton(VirtualKeyCode.LP);
            if (key.Contains("MP"))
                pressButton(VirtualKeyCode.MP);
            if (key.Contains("HP"))
                pressButton(VirtualKeyCode.HP);
            if (key.Contains("LK"))
                pressButton(VirtualKeyCode.LK);
            if (key.Contains("MK"))
                pressButton(VirtualKeyCode.MK);
            if (key.Contains("HK"))
                pressButton(VirtualKeyCode.HK);
        }
        public void pressButton(VirtualKeyCode key)
        {
            if (!pressed.Contains(key))
                pressed.Add(key);
        }
        public void holdButton(VirtualKeyCode key)
        {
            if (!held.Contains(key))
                held.Add(key);
            pressButton(key);
        }
        public void releaseButton(VirtualKeyCode key)
        {
            if (held.Contains(key))
                held.Remove(key);
            if (pressed.Contains(key))
                pressed.Remove(key);
        }
        

    }
}
