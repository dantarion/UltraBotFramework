using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.IO;
using UltraBot;
namespace UltraBotUI
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    public partial class MainWindow : Window
    {

        string[] SEARCH_PATH = { "../../../UltraBot/Bots/" };
        private List<string> BotEntries = new List<string>();

        private IBot bot;
        public MainWindow()
        {
            InitializeComponent();
        }
        [Serializable]
        public class CustomHotKey : HotKey
        {
            public CustomHotKey(string name, Key key, ModifierKeys modifiers, bool enabled)
                : base(key, modifiers, enabled)
            {
                Name = name;
            }

            private string name;
            public string Name
            {
                get { return name; }
                set
                {
                    if (value != name)
                    {
                        name = value;
                        OnPropertyChanged(name);
                    }
                }
            }

            protected override void OnHotKeyPress()
            {
                MessageBox.Show(string.Format("'{0}' has been pressed ({1})", Name, this));

                base.OnHotKeyPress();
            }


            protected CustomHotKey(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
                Name = info.GetString("Name");
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue("Name", Name);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HotKeyHost hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(App.Current.MainWindow));
            hotKeyHost.AddHotKey(new CustomHotKey("ToggleOverlay", Key.F1, ModifierKeys.None, true));
            hotKeyHost.AddHotKey(new CustomHotKey("ToggleBot", Key.F2, ModifierKeys.None, true));
            hotKeyHost.AddHotKey(new CustomHotKey("ChangeBotMode", Key.F3, ModifierKeys.None, true));
            LoadBots();
        }
        private void LoadBots()
        {
            BotEntries.Clear();
            foreach (var searchDir in SEARCH_PATH)
            {
                Bot.AddSearchPath(searchDir);
                foreach (var botfile in Directory.EnumerateFiles(searchDir,"*.cs"))
                {
                    BotEntries.Add(Path.GetFileNameWithoutExtension(botfile));
                }
            }
            BotSelector.ItemsSource = BotEntries;
        }

        private void BotSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bot = Bot.LoadBotFromFile((string)e.AddedItems[0]);
            RefreshBotData();
        }
        private void RefreshBotData()
        {
            StackDisplay.ItemsSource = bot.peekStateStack();
            ComboDisplay.ItemsSource = bot.getComboList();
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if(bot != null)
            {
                if ((sender as RadioButton).Content.Equals("Player 1"))
                    bot.Init(0);
                else
                    bot.Init(1);
                RefreshBotData();
            }
        }

    }
}
