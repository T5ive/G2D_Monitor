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
        private const int LOOP_TASK_INTERVAL = 5;
        private volatile bool running = true;

        public MainForm() => InitializeComponent();

        private void MainForm_Load(object sender, EventArgs e)
        {
            Width = ConfigPlugin.Width;
            Height = ConfigPlugin.Height;
            TopMost = ConfigPlugin.TopMost;
            //Plugin.Register<PlayerPlugin>(); //Debug
            Plugin.Register<MetaPlugin>();
            Plugin.Register<MapPlugin>();
            Plugin.Register<ConfigPlugin>();
            FormClosing += (_, _) => { running = false; Plugin.Close(); };
            ResizeEnd += (_, _) => { ConfigPlugin.Width = Width; ConfigPlugin.Height = Height; };
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
                        Plugin.GameExit();
                    }
                    foreach (var proc in Process.GetProcessesByName(PROCESS_NAME))
                    {
                        if (!proc.HasExited)
                        {
                            Thread.Sleep(INIT_PAUSE_TIME);
                            process = proc;
                            break;
                        }
                    }
                    if (process == null) Thread.Sleep(DETECT_GAME_INTERVAL);
                }
                if (process != null && context == null && (context = Context.Build(process)) == null) Thread.Sleep(INIT_PAUSE_TIME);
                if (context != null)
                {
                    context.Update();
                    Plugin.Update(context);
                    Thread.Sleep(LOOP_TASK_INTERVAL);
                }
            }
            process?.Dispose();
        }
    }
}