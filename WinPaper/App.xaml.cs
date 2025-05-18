using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WinPaper
{
    public partial class App : System.Windows.Application
    {
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool isNewInstance;
            mutex = new Mutex(true, "WinPaperAppMutex", out isNewInstance);
            if (!isNewInstance)
            {
                Shutdown();
                return;
            }

            bool isSilent = e.Args.Contains("--silent");
            var mainWindow = new MainWindow();

            if (isSilent)
            {
                mainWindow.Hide();
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            else
            {
                mainWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
