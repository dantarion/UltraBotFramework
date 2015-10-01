using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraBot
{
    abstract public class InputAdapter
    {
        protected InputAdapter(int player)
        {
            indexer = player;
        }
        protected int indexer = 0;
        protected List<FighterState.Input> pressed = new List<FighterState.Input>();
        protected List<FighterState.Input> last_pressed = new List<FighterState.Input>();
        protected List<FighterState.Input> held = new List<FighterState.Input>();
        public enum KeyMode
        {
            PRESS, HOLD, RELEASE
        }
        public abstract void sendInputs();


        public void pressButton(FighterState.Input key, KeyMode mode)
        {

            if (mode == KeyMode.RELEASE)
            {
                if (held.Contains(key))
                    held.Remove(key);
                if (pressed.Contains(key))
                    pressed.Remove(key);
            }
            else if (mode == KeyMode.HOLD)
            {
                if (!held.Contains(key))
                    held.Add(key);
                if (!pressed.Contains(key))
                    pressed.Add(key);
            }
            else
            {
                if (!pressed.Contains(key))
                    pressed.Add(key);
            }
        }
    }
    class MemoryInputAdapter : InputAdapter
    {
        public MemoryInputAdapter(int player) : base(player)
        {

        }
        public override void sendInputs()
        {
            foreach (var key in last_pressed.ToList())
            {
                if (!pressed.Contains(key) && !held.Contains(key))
                {
                    last_pressed.Remove(key);
                }
            }
            FighterState.Input input = 0;
            foreach (var key in pressed)
            {
                input = input | key;
                if (!last_pressed.Contains(key))
                    last_pressed.Add(key);
            }
            foreach (var key in held)
            {
                input = input | key;
                if (!last_pressed.Contains(key))
                    last_pressed.Add(key);
            }
            var off = indexer == 0 ? 0 : 0x3018-0xC;
            var InputBufferStart = (int)Util.Memory.ReadInt(0x400000 + 0x6A7DF0) + 0x48;
            var InputBufferCurrent = (int)Util.Memory.ReadInt(InputBufferStart - 0x1C) % 0x400;
            Util.Memory.Write(InputBufferStart + 0xC * InputBufferCurrent+off, (int)input);
            pressed.Clear();
        }
    }
    class KeyboardInputAdapter : InputAdapter
    {
        public KeyboardInputAdapter(int player)
            : base(player)
        {

        }
        /// <summary>
        /// These keycodes exist so that we can map them to keyboard or vJoy or whatever.
        /// </summary>
        /// <summary>
        /// TODO: Load this mapping from an XML file, change this function to a dictionary. Support P2 mapping.
        /// </summary>
        private static WindowsInput.VirtualKeyCode map(FighterState.Input key)
        {
            WindowsInput.VirtualKeyCode rawKey;
            switch (key)
            {
                case FighterState.Input.DOWN:
                default:
                    rawKey = WindowsInput.VirtualKeyCode.DOWN;
                    break;
                case FighterState.Input.LEFT:
                    rawKey = WindowsInput.VirtualKeyCode.LEFT;
                    break;
                case FighterState.Input.RIGHT:
                    rawKey = WindowsInput.VirtualKeyCode.RIGHT;
                    break;
                case FighterState.Input.UP:
                    rawKey = WindowsInput.VirtualKeyCode.UP;
                    break;
                case FighterState.Input.LP:
                    rawKey = WindowsInput.VirtualKeyCode.VK_9;
                    break;
                case FighterState.Input.MP:
                    rawKey = WindowsInput.VirtualKeyCode.VK_0;
                    break;
                case FighterState.Input.HP:
                    rawKey = WindowsInput.VirtualKeyCode.OEM_MINUS;
                    break;
                case FighterState.Input.LK:
                    rawKey = WindowsInput.VirtualKeyCode.VK_O;
                    break;
                case FighterState.Input.MK:
                    rawKey = WindowsInput.VirtualKeyCode.VK_P;
                    break;
                case FighterState.Input.HK:
                    rawKey = WindowsInput.VirtualKeyCode.OEM_4;
                    break;
            }
            return rawKey;
        }
        public override void sendInputs()
        {
            if (Util.GetActiveWindowTitle() == "SSFIVAE")
            {
                //For each key that was pressed the previous frame
                foreach (var key in last_pressed.ToList())
                {
                    //If we are pressing it this frame (aka holding it) we need to pick it up
                    if (!pressed.Contains(key) && !held.Contains(key))
                    {
                        WindowsInput.InputSimulator.SimulateKeyUp(map(key));
                        last_pressed.Remove(key);
                    }
                }

                foreach (var key in pressed)
                {
                    var mappedKey = map(key);
                    WindowsInput.InputSimulator.SimulateKeyDown(mappedKey);
                    //If this key isn't in the list of keys to pick up next frame, add it
                    if (!last_pressed.Contains(key))
                        last_pressed.Add(key);
                }
                foreach (var key in held)
                {
                    var mappedKey = map(key);
                    WindowsInput.InputSimulator.SimulateKeyDown(mappedKey);
                    if (!last_pressed.Contains(key))
                        last_pressed.Add(key);
                }
                pressed.Clear();
            }
        }
    }
}
