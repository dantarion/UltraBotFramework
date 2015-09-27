using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UltraBot
{
    /// <summary>
    /// This class controls loading of Bots at runtime. It uses CS-Script to load the .cs file and compile it on the fly!
    /// This allows for the reloading of a Bot in realtime!
    /// </summary>
    public class BotLoader
    {
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
            if (asmHelper != null)
                asmHelper.Dispose();

            //asmHelper = new AsmHelper(CSScript.Load(BotName + ".cs", Guid.NewGuid().ToString(), false));
            asmHelper = new AsmHelper(CSScript.Load(BotName + ".cs", null, false));
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
            return bot;

        }
        /// <summary>
        /// This function sets up the dynamic bot loader with search paths! This is needed to find the "Bot" folder at runtime.
        /// </summary>
        /// <param name="Dir"></param>
        public static void AddSearchPath(string Dir)
        {
            CSScript.GlobalSettings.AddSearchDir(Dir);
        }
        private static void LoadCombos(Bot bot, string XMLFilename)
        {
            var xmldoc = new XmlDocument();
            xmldoc.Load(XMLFilename);
            foreach (XmlNode comboXml in xmldoc.DocumentElement.SelectNodes("//Combo"))
            {
                var combo = new Combo();
                ComboType tmp = 0;
                string stInput = "";
                if (comboXml.ParentNode.LocalName == "Starter")
                {
                    foreach (XmlAttribute attr in comboXml.ParentNode.Attributes)
                    {
                        PropertyInfo propertyInfo = combo.GetType().GetProperty(attr.Name);
                        if (propertyInfo.PropertyType.IsEnum)
                            propertyInfo.SetValue(combo, Enum.Parse(typeof(ComboType), attr.Value));
                        else
                            propertyInfo.SetValue(combo, Convert.ChangeType(attr.Value, propertyInfo.PropertyType), null);
                    }
                    tmp = combo.Type;
                    stInput = combo.Input;

                }

                foreach(XmlAttribute attr in comboXml.Attributes)
                {
                    PropertyInfo propertyInfo = combo.GetType().GetProperty(attr.Name);
                    if(propertyInfo.PropertyType.IsEnum)
                        propertyInfo.SetValue(combo, Enum.Parse(typeof(ComboType), attr.Value));
                    else
                        propertyInfo.SetValue(combo, Convert.ChangeType(attr.Value, propertyInfo.PropertyType), null);
                }
                combo.Type = combo.Type | tmp;
                if(stInput != "")
                    combo.Input = stInput + "."+combo.Input;
                bot.getComboList().Add(combo);
            }
        }
    }
}
