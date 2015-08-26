using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsInput;
using CSScriptLibrary;
namespace UltraBot
{
    /// <summary>
    /// This is a dummy interface used for the CS-Script integration
    /// </summary>
    public interface IBot
    {
         void Init(int index);
         List<Combo> getComboList();
         void Run();
         BotAIState DefaultState();
         string getStatus();
    }

    public class Bot : MarshalByRefObject, IBot 
    {
        protected Bot()
        {
            currentState = DefaultState();
        }
        /// <summary>
        /// This function sets up the dynamic bot loader with search paths!
        /// </summary>
        /// <param name="Dir"></param>
        public static void AddSearchPath(string Dir)
        {
            CSScript.GlobalSettings.AddSearchDir(Dir);
        }
        private List<Type> TriggerStates = new List<Type>();
        /// <summary>
        /// Registers a state that can trigger itself at any time. The state must inherit from BotAIState and implement Trigger() static method. 
        /// See DefendState for an example.
        /// </summary>
        /// <param name="t"></param>
        protected void RegisterState(Type t)
        {
            if (t.IsSubclassOf(typeof(BotAIState)) && !TriggerStates.Contains(t))
                TriggerStates.Add(t);
        }
        static private AsmHelper asmHelper = null;
        /// <summary>
        /// This function loads, compiles, and instantiates a bot on the fly.
        /// </summary>
        /// <param name="BotName">The name of the bot. Should be in the a folder that has been added to the search path
        /// via AddSearchPath, and named BotName.cs</param>
        /// <returns></returns>
        public static Bot LoadBotFromFile(string BotName)
        {

            Bot bot = null;
            //TODO FIX HOTRELOAD
 
            {
                if (asmHelper != null)
                    asmHelper.Dispose();

                asmHelper = new AsmHelper(CSScript.Load(BotName + ".cs", Guid.NewGuid().ToString(), false));
                var tmp2 = asmHelper.CreateObject(BotName);
                bot = tmp2 as Bot;

                foreach (var dir in CSScript.GlobalSettings.SearchDirs.Split(';'))//We look for an xml file containing a list of button combos. See KenBot.xml for an example
                {
                    var fname = System.IO.Path.Combine(dir, BotName + ".xml");
                    if (System.IO.File.Exists(fname))
                    {
                        LoadCombos((Bot)bot, fname);
                    }
                }
            }
            return bot;

        }
        #region Combo System
        private static void LoadCombos(Bot bot, string XMLFilename)
        {
            var xmldoc = new XmlDocument();
            xmldoc.Load(XMLFilename);
            foreach (XmlNode comboXml in xmldoc.DocumentElement.SelectNodes("//Combo"))
            {
                var combo = new Combo();
                combo.Type = (ComboType)Enum.Parse(typeof(ComboType), comboXml.Attributes["Type"].Value);
                combo.Startup = Int32.Parse(comboXml.Attributes["Startup"].Value);
                combo.XMin = float.Parse(comboXml.Attributes["XMin"].Value);
                combo.XMax = float.Parse(comboXml.Attributes["XMax"].Value);
                combo.YMin = float.Parse(comboXml.Attributes["YMin"].Value);
                combo.YMax = float.Parse(comboXml.Attributes["YMax"].Value);
                combo.EXMeter = Int32.Parse(comboXml.Attributes["EXMeter"].Value);
                combo.Input = comboXml.Attributes["Input"].Value;
                bot.comboList.Add(combo);
            }
        }
        protected float scoreCombo(Combo combo)
        {
            float score = 1.0f;
            //Killers
            
            if (!combo.Type.HasFlag(ComboType.ANTIAIR) && (enemyState.Y != 0 || enemyState.ScriptName.Contains("2JUMP")))
                return 0;

            if (!combo.Type.HasFlag(ComboType.GROUND) && (enemyState.Y == 0 && !enemyState.ScriptName.Contains("2JUMP")))
                return 0;
            if(myState.Meter < combo.EXMeter)
                return 0;//We don't have the meter
            if(combo.EXMeter > 0)
                score *= (float)myState.Meter / (float)combo.EXMeter;

            if (combo.Type.HasFlag(ComboType.ULTRA) && myState.Revenge < 0x190/2)
                return 0;//We don't have ultra
            else
                score *= (float)myState.Revenge;
            //Corner
            float cornerDistance;
            if (myState.XDistance > 0)
                cornerDistance = Math.Abs(7.5f + myState.X);//Facing Left
            else
                cornerDistance = Math.Abs(-7.5f + myState.X);//Facing right
            if (combo.Type.HasFlag(ComboType.CORNER) && cornerDistance > 2.5)
                return 0;
            if (combo.Type.HasFlag(ComboType.MIDSCREEN) && cornerDistance < 2.5)
                return 0;
            if (Math.Abs(myState.XDistance) <= combo.XMax && Math.Abs(myState.XDistance) >= combo.XMin)
                score += 100;//We are already in range
            //TODO IF WE ARE ALMOST IN RANGE
            //TODO GROUNDED.ANTIAIR.POKE.THROWF setup
            if (combo.Type.HasFlag(ComboType.DEBUG))
                return float.MaxValue;
            return score;
        }
        protected void scoreCombos(int startup = Int32.MaxValue)
        {
            foreach (var combo in comboList)
                combo.Score = scoreCombo(combo);
        }
		private List<Combo> comboList = new List<Combo>();
        public List<Combo> getComboList()
        {

            return comboList;
        }
        #endregion

		/// <summary>
        /// This sets up the bot to know which side it is playing on.
        /// </summary>
        public void Init(int index)
        {
            myState = FighterState.getFighter(index);
            enemyState = FighterState.getFighter(index == 0 ? 1 : 0);
        }

        public FighterState myState;
        public FighterState enemyState;
        private string _status = "";
        public string getStatus()
        {
            return _status;
        }
        public virtual BotAIState DefaultState()
        {
            return new IdleState();
        }
        /// <summary>
        /// This function does the magic, and makes the bot actually work. It only handles input when the window is focused.
        /// TODO: MatchState also has some of this current window logic, feels redundant
        /// </summary>
        /// 
        public virtual void Run()
        {
            //Setup some derived variables.
            myState.XDistance = myState.X - enemyState.X;
            myState.YDistance = myState.Y - enemyState.Y;
            scoreCombos();
            foreach(var t in TriggerStates)
            {
                var method = t.GetMethod("Trigger");
                var result = method.Invoke(null,new object[]{this});
                if (result != null && currentState.GetType() != t)
                {
                    changeState(result as BotAIState);
                    break;
                }
            }

            _status = currentState.Process(this);
            if (currentState.isFinished())
            {
                changeState(DefaultState());
                _status = currentState.Process(this);
            }
            
            if (Util.GetActiveWindowTitle() == "SSFIVAE")
            {
                //For each key that was pressed the previous frame
                foreach (var key in last_pressed.ToList())
                {
                    //If we are pressing it this frame (aka holding it) we need to pick it up
                    if (!pressed.Contains(key) && !held.Contains(key))
                    {
                        //Console.WriteLine(" \t{0} UP {1}", MatchState.getInstance().FrameCounter, key);
                        //WindowsInput.InputSimulator.SimulateKeyUp(map(key));
                        last_pressed.Remove(key);
                    }
                }
                //For each key that was pressed this frame, we need to send keydown
                FighterState.Input input = 0;
                foreach (var key in pressed)
                {
                    //Console.WriteLine(" \t{0} DOWN {1}", MatchState.getInstance().FrameCounter, key);
                    //var mappedKey = map(key);
                    //WindowsInput.InputSimulator.SimulateKeyDown(mappedKey);
                    input = input | key;
                    //If this key isn't in the list of keys to pick up next frame, add it
                    if(!last_pressed.Contains(key))
                        last_pressed.Add(key);
                }
                foreach (var key in held)
                {
                    //Console.WriteLine(" \t{0} DOWN {1}", MatchState.getInstance().FrameCounter, key);
                    //var mappedKey = map(key);
                    //WindowsInput.InputSimulator.SimulateKeyDown(mappedKey);
                    //If this key isn't in the list of keys to pick up next frame, add it
                    if (!last_pressed.Contains(key))
                        last_pressed.Add(key);
                }
                myState.PressInput(input);
                
                pressed.Clear();
            }
        }


        #region State Management
        private BotAIState previousState;
        private BotAIState currentState;
        /// <summary>
        /// This function runs before any state.
        /// By overriding this function, you can have checks that force the bot into an arbitrary state based on triggers.
        /// Eventually I want to actually use a event/listener pattern here, and allow states to be registered with autotriggers that automatically enter them.
        /// I.E. ThrowTechState would have a Statecheck that selftriggers when the bot is thrown. TODO
        /// </summary>
        public virtual void StateCheck()
        {

        }
        /// <summary>
        /// This ends the current state permanantly and changes to a new state.
        /// 
        /// </summary>
        /// <param name="nextState"></param>
        public void changeState(BotAIState nextState)
        {
            previousState = currentState;
            currentState = nextState;
        }
        #endregion
        #region Input Management
        private List<FighterState.Input> pressed = new List<FighterState.Input>();
        private List<FighterState.Input> last_pressed = new List<FighterState.Input>();
        private List<FighterState.Input> held = new List<FighterState.Input>();
        /// <summary>
        /// These keycodes exist so that we can map them to keyboard or vJoy or whatever.
        /// </summary>
		/// <summary>
        /// TODO: Load this mapping from an XML file, change this function to a dictionary. Support P2 mapping.
        /// </summary>
        private static WindowsInput.VirtualKeyCode map(VirtualKeyCode key)
        {
            WindowsInput.VirtualKeyCode rawKey;
            switch (key)
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
                    rawKey = WindowsInput.VirtualKeyCode.OEM_MINUS;
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
        /// Mapping between game buttons that is eventually mapped to windows key codes
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
        private FighterState.Input Forward()
        {
            if(myState.XDistance < 0)
                return FighterState.Input.FORWARD;
            return FighterState.Input.BACK;
        }
        
        private FighterState.Input Back()
        {
            if(myState.XDistance < 0)
                return FighterState.Input.BACK;
            return FighterState.Input.FORWARD;
        }
        public void pressButton(string key)
        {
            KeyMode kmode = KeyMode.PRESS;
            if (key.Contains('_'))
                kmode = KeyMode.HOLD;
            if (key.Contains('+'))
                kmode = KeyMode.RELEASE;
            if (key.Contains("2"))
                pressButton(FighterState.Input.DOWN, kmode);
            if (key.Contains("6"))
                pressButton(this.Forward(), kmode);
            if (key.Contains("4"))
                pressButton(this.Back(), kmode);
            if (key.Contains("8"))
                pressButton(FighterState.Input.UP, kmode);
			if (key.Contains("1"))	
			{
                pressButton(this.Back(), kmode);
                pressButton(FighterState.Input.DOWN, kmode);
			}
			if (key.Contains("3"))	
			{
                pressButton(this.Forward(), kmode);
                pressButton(FighterState.Input.DOWN, kmode);
			}
			if (key.Contains("7"))	
			{
                pressButton(this.Back(), kmode);
                pressButton(FighterState.Input.UP, kmode);
			}
			if (key.Contains("9"))	
			{
                pressButton(this.Forward(), kmode);
                pressButton(FighterState.Input.UP, kmode);
			}
			
            if (key.Contains("LP"))
                pressButton(FighterState.Input.LP, kmode);
            if (key.Contains("MP"))
                pressButton(FighterState.Input.MP, kmode);
            if (key.Contains("HP"))
                pressButton(FighterState.Input.HP, kmode);
            if (key.Contains("LK"))
                pressButton(FighterState.Input.LK, kmode);
            if (key.Contains("MK"))
                pressButton(FighterState.Input.MK, kmode);
            if (key.Contains("HK"))
                pressButton(FighterState.Input.HK, kmode);
        }
        public enum KeyMode
        {
            PRESS,HOLD,RELEASE
        }
        public void pressButton(FighterState.Input key, KeyMode mode)
        {
            
            if(mode == KeyMode.RELEASE)
            {
                if (held.Contains(key))
                    held.Remove(key);
                if (pressed.Contains(key))
                    pressed.Remove(key);
            }
            else if(mode == KeyMode.HOLD)
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
        #endregion

 
    }
}
