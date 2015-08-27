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
    public class Bot : MarshalByRefObject
    {
        public Bot()
        {
            currentState = DefaultState();
        }
        public delegate BotAIState AITrigger(Bot bot);
        private List<AITrigger> TriggerStates = new List<AITrigger>();
        /// <summary>
        /// Registers a state that can trigger itself at any time. The state must inherit from BotAIState and implement Trigger() static method. 
        /// See DefendState for an example.
        /// </summary>
        /// <param name="t"></param>
        protected void RegisterState(AITrigger t)
        {
            if (!TriggerStates.Contains(t))
                TriggerStates.Add(t);
        }
        #region Combo System
        protected virtual float scoreCombo(Combo combo, int startup = Int32.MaxValue)
        {
            return 1;
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
            StateCheck();
            _status = currentState.Process(this);
            if (currentState.isFinished())
            {
                changeState(DefaultState());
                _status = currentState.Process(this);
            }

            inputAdapter.sendInputs();
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
            foreach (var t in TriggerStates)
            {
                var result = t(this);
                if (result != null && currentState.GetType() != result.GetType())
                {
                    changeState(result as BotAIState);
                    break;
                }
            }
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
        private InputAdapter inputAdapter = new MemoryInputAdapter();
        public void pressButton(string key, InputAdapter.KeyMode kmode = InputAdapter.KeyMode.PRESS)
        {

            if (key.Contains('_'))
                kmode = InputAdapter.KeyMode.HOLD;
            if (key.Contains('+'))
                kmode = InputAdapter.KeyMode.RELEASE;

            /* Directional inputs. Have to account for facing direction for back and forward */
            if (key.Contains("2"))
                inputAdapter.pressButton(FighterState.Input.DOWN, kmode);
            if (key.Contains("6"))
                if (myState.XDistance < 0)
                    inputAdapter.pressButton(FighterState.Input.RIGHT, kmode);
                else
                    inputAdapter.pressButton(FighterState.Input.LEFT, kmode);
            if (key.Contains("4"))
                if (myState.XDistance > 0)
                    inputAdapter.pressButton(FighterState.Input.RIGHT, kmode);
                else
                    inputAdapter.pressButton(FighterState.Input.LEFT, kmode);
            if (key.Contains("8"))
                inputAdapter.pressButton(FighterState.Input.UP, kmode);
            /* Diagonal shorthand */
            if (key.Contains("1"))
            {
                pressButton("4", kmode);
                pressButton("2", kmode);
            }
            if (key.Contains("3"))
            {
                pressButton("6", kmode);
                pressButton("2", kmode);
            }
            if (key.Contains("7"))
            {
                pressButton("4", kmode);
                pressButton("8", kmode);
            }
            if (key.Contains("9"))
            {
                pressButton("6", kmode);
                pressButton("8", kmode);
            }
            /* Attack buttons */
            if (key.Contains("LP"))
                inputAdapter.pressButton(FighterState.Input.LP, kmode);
            if (key.Contains("MP"))
                inputAdapter.pressButton(FighterState.Input.MP, kmode);
            if (key.Contains("HP"))
                inputAdapter.pressButton(FighterState.Input.HP, kmode);
            if (key.Contains("LK"))
                inputAdapter.pressButton(FighterState.Input.LK, kmode);
            if (key.Contains("MK"))
                inputAdapter.pressButton(FighterState.Input.MK, kmode);
            if (key.Contains("HK"))
                inputAdapter.pressButton(FighterState.Input.HK, kmode);
        }
        #endregion

 
    }
}
