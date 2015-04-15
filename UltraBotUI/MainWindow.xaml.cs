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
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.IO;
using UltraBot;
using System.ComponentModel;
using DX9OverlayAPIWrapper;
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
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private IBot bot;

        MatchState ms = new MatchState();
        FighterState f1 = FighterState.getFighter(0);
        FighterState f2 = FighterState.getFighter(1);
        TextLabel roundTimer;
        TextLabel player1;
        TextLabel player2;

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
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            LoadBots();
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(
        () => BotSelector_SelectionChanged(source, null)));
            
        }
        private void LoadBots()
        {
            BotEntries.Clear();
            foreach (var searchDir in SEARCH_PATH)
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = searchDir;
                watcher.NotifyFilter =NotifyFilters.LastWrite;
                watcher.Filter = "*.*";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;
                Bot.AddSearchPath(searchDir);
                foreach (var botfile in Directory.EnumerateFiles(searchDir,"*.cs"))
                {
                    BotEntries.Add(Path.GetFileNameWithoutExtension(botfile));
                }
            }
            BotSelector.ItemsSource = BotEntries;
            BotSelector.SelectedIndex = 0;
        }

        private void BotSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StatusLabel.Content = "Loading " + (string)BotSelector.SelectedValue +"...";
            bot = Bot.LoadBotFromFile((string)BotSelector.SelectedValue);
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
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                ms.Update();
                f1.UpdatePlayerState();
                f2.UpdatePlayerState(); 
                roundTimer.Text = String.Format("Frame:{0}", ms.FrameCounter);
                UpdateOverlay(player1, f1);
                UpdateOverlay(player2, f2);
                bot.Run();
            }
        }
        private void backgroundWorker_ProgressChanged(object sender,  ProgressChangedEventArgs e)
        {

        }

        private void OverlayEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (OverlayEnabled.IsChecked.Value)
            {
                StatusLabel.Content = "Enabling Overlay...";
                DX9Overlay.SetParam("process", "SSFIV.exe");
                DX9Overlay.DestroyAllVisual();
                roundTimer = new TextLabel("Consolas", 10, TypeFace.NONE, new System.Drawing.Point(390, 0), Color.White, "", true, true);
                player1 = new TextLabel("Consolas", 10, TypeFace.NONE, new System.Drawing.Point(90, 0), Color.White, "", true, true);
                player2 = new TextLabel("Consolas", 10, TypeFace.NONE, new System.Drawing.Point(480, 0), Color.White, "", true, true);
            }
            else
            {
                StatusLabel.Content = "Disabling Overlay...";
                DX9Overlay.DestroyAllVisual();
            }
        }
        private void SetupOverlay()
        {
            
        }
        private static void UpdateOverlay(TextLabel label, FighterState f)
        {
            
            label.Text = String.Format("X={0,-7} Y={1,-7} XVel={12,-7} YVel={13,-7}\n{2,-15} F:{3,-3}\nACT:{4,-3} ENDACT:{5,-3} IASA:{6,-3} TOT:{7,-3}\n{8,-10} {9,-10} {10,-10} {11:X}\n{14}",
                f.X, f.Y, f.ScriptName, f.ScriptFrame, f.ScriptFrameHitboxStart, f.ScriptFrameHitboxEnd, f.ScriptFrameIASA, f.ScriptFrameTotal, f.State, f.AState, f.StateTimer, f.RawState, f.XVelocity, f.YVelocity, String.Join(", ", f.ActiveCancelLists));
        }

    }
}
