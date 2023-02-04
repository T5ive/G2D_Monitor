using G2D_Monitor.Manager;
using G2D_Monitor.Plugins;
using System.Diagnostics;

namespace G2D_Monitor
{
    public partial class MainForm : Form
    {
        public static readonly MainForm Instance = new();
        private const string PROCESS_NAME = "Goose Goose Duck";
        private const int INIT_PAUSE_TIME = 1000;
        private const int DETECT_GAME_INTERVAL = 100;
        private volatile bool running;

        public MainForm() => InitializeComponent();

        private void MainForm_Load(object sender, EventArgs e)
        {
            Plugin.Register<PlayerPlugin>();
            Plugin.Register<MetaPlugin>();
            FormClosing += (_, _) => running = false;
            Watch();
        }

        public void Watch()
        {
            running = true;
            new Task(() =>
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
            }).Start();
        }
    }
}