using System.Diagnostics;

namespace G2D_Monitor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            try
            {
                Process.EnterDebugMode();
            }
            catch
            {
                MessageBox.Show("Please run this application as Administrator!", "Error");
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(MainForm.Instance);
        }
    }
}