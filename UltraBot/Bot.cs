using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
namespace UltraBot
{
    public class Bot
    {
        public Bot(int index)
        {
            myState = FighterState.getFighter(index);
            enemyState = FighterState.getFighter(index == 0 ? 1 : 0);
        }
        public FighterState myState;
        public FighterState enemyState;
        public List<BotState> stateStack = new List<BotState>();
        public List<VirtualKeyCode> pressed = new List<VirtualKeyCode>();
        public List<VirtualKeyCode> last_pressed = new List<VirtualKeyCode>();
        public List<VirtualKeyCode> held = new List<VirtualKeyCode>();
        public BotState currentState;
        public BotState previousState;
        public virtual void Run()
        {
            currentState.Run();
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

    }
    return rawKey;
}
 
        /// <summary>
        /// This changes state.
        /// </summary>
        /// <param name="nextState"></param>
        public void changeState(BotState nextState)
        {
            previousState = currentState;
            currentState = nextState;
        }
        /// <summary>
        /// This can be used to return to a previous state.
        /// </summary>
        public void popState()
        {
            changeState(stateStack[0]);
            stateStack.RemoveAt(0);
        }
        /// <summary>
        /// This function can be used to switch to a state
        /// while adding the old state onto the stack. This lets you return to that state by calling popState() from the new state 
        /// </summary>
        /// <param name="nextState"></param>
        public void pushState(BotState nextState)
        {
            stateStack.Add(currentState);
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
