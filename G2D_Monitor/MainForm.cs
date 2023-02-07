using G2D_Monitor.Game;
using G2D_Monitor.Manager;
using G2D_Monitor.Plugins;
using System.Diagnostics;

namespace G2D_Monitor
{
    public partial class MainForm : Form
    {
        public static readonly MainForm Instance = new();
        public const string PROCESS_NAME = "Goose Goose Duck";
        private const int INIT_PAUSE_TIME = 1000;
        private const int DETECT_GAME_INTERVAL = 100;
        private volatile bool running = true;

        public MainForm() => InitializeComponent();

        private void MainForm_Load(object sender, EventArgs e)
        {
            TopMost = ConfigPlugin.TopMost;
            //Plugin.Register<PlayerPlugin>(); //For Debug Only
            Plugin.Register<MetaPlugin>();
            Plugin.Register<MapPlugin>();
            Plugin.Register<ConfigPlugin>();
            FormClosing += (_, _) => { running = false; Plugin.Close(); };
            Multiboxing.Click += (_, _) => MutexKiller.Run();
            new Task(Watch).Start();
        }

        private void Watch()
        {
            Process? process = null;
            Context? context = null;
            while (running)
            {
                if (process == null || process.HasExited)
                {
                    if (process != null)
                    {
                        process.Dispose();
                        process = null;
                        context = null;
                        Plugin.Update(null);
                    }
                    foreach (var proc in Process.GetProcessesByName(PROCESS_NAME))
                    {
                        if (!proc.HasExited)
                        {
                            Thread.Sleep(INIT_PAUSE_TIME);
                            context = new(process = proc);
                            break;
                        }
                    }
                    if (process == null) Thread.Sleep(DETECT_GAME_INTERVAL);
                }

                if (context != null)
                {
                    context.Update();
                    Plugin.Update(context);
                }
            }
            process?.Dispose();
        }
    }
}