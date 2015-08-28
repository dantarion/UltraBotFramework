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
using System.Threading;
namespace UltraBotUI
{
    public class LogEntry
    {   
        public string Message { get; set; }
        public string BotScriptInfo { get; set; }
        public string EnemyScriptInfo { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {

        string[] SEARCH_PATH = { "../../../UltraBot/Bots/" };

        private List<string> BotEntries = new List<string>();
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private Bot bot;

        MatchState ms = MatchState.getInstance();
        FighterState f1 = FighterState.getFighter(0);
        FighterState f2 = FighterState.getFighter(1);
        TextLabel roundTimer;
        TextLabel player1;
        TextLabel player2;

        public MainWindow()
        {
            InitializeComponent();
        }
        #region Hotkeys
        [Serializable]
        public class CustomHotKey : HotKey
        {
            public CustomHotKey(string name, Key key, ModifierKeys modifiers, bool enabled, MainWindow w)
                : base(key, modifiers, enabled)
            {
                Name = name;
                window = w;
            }
            private MainWindow window;
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
                if (Name == "ToggleOverlay")
                    window.OverlayEnabled.IsChecked = !window.OverlayEnabled.IsChecked.Value;
                else if (Name == "ToggleBot")
                    window.BotEnabled.IsChecked = !window.BotEnabled.IsChecked.Value;
                else
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
        #endregion
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HotKeyHost hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(App.Current.MainWindow));
            hotKeyHost.AddHotKey(new CustomHotKey("ToggleOverlay", Key.F1, ModifierKeys.None, true,this));
            hotKeyHost.AddHotKey(new CustomHotKey("ToggleBot", Key.F2, ModifierKeys.None, true, this));
            hotKeyHost.AddHotKey(new CustomHotKey("ChangeBotMode", Key.F3, ModifierKeys.None, true, this));
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            LoadBots();
            var le = new LogEntry();
            le.Message = "Loaded!";
            log.Insert(0, le);
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ComboDisplay.Items.Refresh();
        }
        private void FolderOnChanged(object source, FileSystemEventArgs e)
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
                watcher.Changed += new FileSystemEventHandler(FolderOnChanged);
                watcher.EnableRaisingEvents = true;
                BotLoader.AddSearchPath(searchDir);
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
            try
            {
                bot = BotLoader.LoadBotFromFile((string)BotSelector.SelectedValue);
                BotSelector.Items.Refresh();
                RefreshBotData();
                bot.Init(0);
            }
            catch(Exception err)
            {
                StatusLabel.Content = err.ToString();
            }
            

        }
        private List<LogEntry> log = new List<LogEntry>();
        private void RefreshBotData()
        {
            Log.ItemsSource = log;
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
            WorkerArgs args = (WorkerArgs)e.Argument;
            while (!backgroundWorker.CancellationPending)
            {
                ms.Update();
                f1.UpdatePlayerState();
                f2.UpdatePlayerState();
                if (args.runBot)
                    bot.Run();
                backgroundWorker.ReportProgress(0, bot.getStatus());
                if (args.runOverlay)
                {
                    roundTimer.Text = bot.getStatus();
                    UpdateOverlay(player1, f1);
                    UpdateOverlay(player2, f2);
                }
                
            }
            e.Cancel = true;
        }
        private void backgroundWorker_RunWorkerCompleted
            ( object sender, RunWorkerCompletedEventArgs e )
        {
            restartWorker();
        }
        private void backgroundWorker_ProgressChanged(object sender,  ProgressChangedEventArgs e)
        {
            StatusLabel.Content = String.Format("XDistance:{0} YPos:{1}",bot.myState.XDistance,bot.enemyState.Y);
            if (log.Count == 0 || !log[0].Message.Equals(bot.getStatus()) )
            {
                var le = new LogEntry();
                le.Message = bot.getStatus();
                le.BotScriptInfo = String.Format("{0}:{1} - {2}", bot.myState.ScriptName, bot.myState.ScriptFrame, bot.myState.State);
                le.EnemyScriptInfo = String.Format("{0}:{1} - {2}", bot.enemyState.ScriptName, bot.enemyState.ScriptFrame, bot.enemyState.State);
                log.Insert(0, le);
                Log.Items.Refresh();
               
            }
            while (log.Count > 200)
                log.RemoveAt(log.Count - 1);
            RefreshBotData();
            

        }
        struct WorkerArgs
        {
            public bool runOverlay;
            public bool runBot;
        }
        private void restartWorker()
        {
            Util.Init();
            var args = new WorkerArgs();
            args.runOverlay = OverlayEnabled.IsChecked.Value;
            args.runBot = BotEnabled.IsChecked.Value;
            if (backgroundWorker.IsBusy)
                backgroundWorker.CancelAsync();
            else
                backgroundWorker.RunWorkerAsync(args);
            
        }
        private void BotEnabled_Checked(object sender, RoutedEventArgs e)
        {

            Dispatcher.BeginInvoke((Action)(
        () => BotSelector_SelectionChanged(sender, null)));
            
        
            restartWorker();

        }
        private void OverlayEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (OverlayEnabled.IsChecked.Value)
            {
                StatusLabel.Content = "Enabling Overlay...";
                DX9Overlay.SetParam("process", "SSFIV.exe");
                DX9Overlay.DestroyAllVisual();
                roundTimer = new TextLabel("Consolas", 10, TypeFace.NONE, new System.Drawing.Point(4, 700), Color.White, "", true, true);
                player1 = new TextLabel("Consolas", 10, TypeFace.NONE, new System.Drawing.Point(5, 0), Color.White, "", true, true);
                player2 = new TextLabel("Consolas", 10, TypeFace.NONE, new System.Drawing.Point(480, 0), Color.White, "", true, true);
                restartWorker();
            }
            else
            {
                StatusLabel.Content = "Disabling Overlay...";
                DX9Overlay.DestroyAllVisual();
                restartWorker();
            }
        }
        private static void UpdateOverlay(TextLabel label, FighterState f)
        {
            
            label.Text = String.Format("X={0,-7} Y={1,-7} XVel={12,-7} YVel={13,-7}\n{2,-15} F:{3,-3}\nACT:{4,-3} ENDACT:{5,-3} IASA:{6,-3} TOT:{7,-3}\n{8,-10} {9,-10} {10,-10} {11:X}\n{14}",
                f.X, f.Y, f.ScriptName, f.ScriptFrame, f.ScriptFrameHitboxStart, f.ScriptFrameHitboxEnd, f.ScriptFrameIASA, f.ScriptFrameTotal, f.State, f.AState, f.StateTimer, f.RawState, f.XVelocity, f.YVelocity, String.Join(", ", f.ActiveCancelLists));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Remove overlay from the game
            DX9Overlay.DestroyAllVisual();
        }

        private void ComboDisplay_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Type")
                e.Cancel = true;
        }
    }
}
