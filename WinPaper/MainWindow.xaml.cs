using System;
using System.IO;
using File = System.IO.File;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using IWshRuntimeLibrary;
using System.Reflection;
using System.Diagnostics;

namespace WinPaper
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly WallpaperManager wallpaperManager = new();
        private bool isRunning = false;
        private NotifyIcon trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            RegisterInStartup();
            RegisterInAppsList();
            CreateStartMenuShortcut();

            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string iconPath = Path.Combine(exeDir, "winpaper.ico");

            trayIcon = new NotifyIcon
            {
                Icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application,
                Visible = true,
                Text = "WinPaper - Fond d'écran dynamique"
            };

            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add("Afficher", null, (s, e) =>
            {
                ShowWindow();
            });

            trayIcon.ContextMenuStrip.Items.Add("Quitter", null, (s, e) =>
            {
                trayIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });

            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ShowWindow();
                }
            }; 

            SettingsManager.LoadSettings(out var url, out var dynamicMode, out var interval, out var wasRunning);
            UrlTextBox.Text = url;
            DynamicCheckBox.IsChecked = dynamicMode;
            IntervalTextBox.Text = interval.ToString();

            if (wasRunning && dynamicMode)
            {
                cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(() => DynamicWallpaperService.Start(url, interval, wallpaperManager, cancellationTokenSource.Token));
                ToggleButton.Content = "Arrêter";
                ToggleButton.Background = System.Windows.Media.Brushes.Red;
                UrlTextBox.IsEnabled = false;
                DynamicCheckBox.IsEnabled = false;
                IntervalTextBox.IsEnabled = false;
                isRunning = true;
            }
            this.Closing += MainWindow_Closing;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide(); 
        }

        private void RegisterInStartup()
        {
            try
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key?.SetValue("WinPaper", $"\"{exePath}\" --silent");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout au démarrage : " + ex.Message);
            }
        }

        private void RegisterInAppsList()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\WinPaper");
                key.SetValue("DisplayName", "WinPaper - Fond d'écran dynamique");
                key.SetValue("Publisher", "MebApp");
                key.SetValue("DisplayVersion", "1.0");
                key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                key.SetValue("DisplayIcon", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                key.SetValue("UninstallString", $"\"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}\" --uninstall");
                key.SetValue("InstallLocation", AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement dans les applications : " + ex.Message);
            }
        }

        private void CreateStartMenuShortcut()
        {
            string shortcutPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs",
                "WinPaper.lnk");

            string exePath = Process.GetCurrentProcess().MainModule.FileName;

            if (!System.IO.File.Exists(shortcutPath))
            {
                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "WinPaper - Fond d'écran dynamique";
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.IconLocation = exePath;
                shortcut.Save();
            }
        }

        private async void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                cancellationTokenSource?.Cancel();
                ToggleButton.Content = "Appliquer";
                ToggleButton.Background = System.Windows.Media.Brushes.Green;
                UrlTextBox.IsEnabled = true;
                DynamicCheckBox.IsEnabled = true;
                IntervalTextBox.IsEnabled = DynamicCheckBox.IsChecked == true;
                isRunning = false;
                return;
            }

            string url = UrlTextBox.Text;
            bool isDynamic = DynamicCheckBox.IsChecked == true;
            int interval = 300;

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Veuillez entrer un lien d'image valide.");
                return;
            }

            if (isDynamic)
            {
                if (!int.TryParse(IntervalTextBox.Text, out interval) || interval < 30 || interval > 43200)
                {
                    MessageBox.Show("Fréquence invalide. Doit être entre 30 et 43200 secondes.");
                    return;
                }
            }

            if (isDynamic)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(() => DynamicWallpaperService.Start(url, interval, wallpaperManager, cancellationTokenSource.Token));
                ToggleButton.Content = "Arrêter";
                ToggleButton.Background = System.Windows.Media.Brushes.Red;
                UrlTextBox.IsEnabled = false;
                DynamicCheckBox.IsEnabled = false;
                IntervalTextBox.IsEnabled = false;
                isRunning = true;
                SettingsManager.SaveSettings(url, isDynamic, interval, isRunning);
            }
            else
            {
                await wallpaperManager.SetWallpaperAsync(url);
                ToggleButton.Content = "Appliquer";
                ToggleButton.Background =   System.Windows.Media.Brushes.Green;
                UrlTextBox.IsEnabled = true;
                DynamicCheckBox.IsEnabled = true;
                IntervalTextBox.IsEnabled = true;
                isRunning = false;
                SettingsManager.SaveSettings(url, isDynamic, interval, isRunning);
            }
        }

        private void DynamicCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
                IntervalTextBox.IsEnabled = true;
        }

        private void DynamicCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IntervalTextBox.IsEnabled = false;
        }
    }

    public class WallpaperManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        public async Task<bool> SetWallpaperAsync(string imageUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.jpg");
                await File.WriteAllBytesAsync(tempPath, imageBytes);
                return SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch
            {
                MessageBox.Show("Erreur lors du téléchargement ou de l'application de l'image.");
                return false;
            }
        }
    }

    public static class SettingsManager
    {
        private const string FilePath = "settings.json";

        public static void SaveSettings(string url, bool isDynamic, int interval, bool isRunning)
        {
            var settings = new Settings
            {
                Url = url,
                IsDynamic = isDynamic,
                Interval = interval,
                IsRunning = isRunning
            };
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings));
        }

        public static void LoadSettings(out string url, out bool isDynamic, out int interval, out bool isRunning)
        {
            if (!File.Exists(FilePath))
            {
                url = "";
                isDynamic = false;
                interval = 300;
                isRunning = false;
                return;
            }

            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath));
            url = settings?.Url ?? "";
            isDynamic = settings?.IsDynamic ?? false;
            interval = settings?.Interval ?? 300;
            isRunning = settings?.IsRunning ?? false;
        }
    }

    public static class DynamicWallpaperService
    {
        public static async Task Start(string url, int intervalSeconds, WallpaperManager manager, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await manager.SetWallpaperAsync(url);
                await Task.Delay(intervalSeconds * 1000, token);
            }
        }
    }

    public class Settings
    {
        public string Url { get; set; }
        public bool IsDynamic { get; set; }
        public int Interval { get; set; }
        public bool IsRunning { get; set; } 
    }
}
