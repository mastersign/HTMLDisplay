using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;


namespace Mastersign.HtmlDisplay
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string FilePath { get; }

        private bool ValidFile => FilePath != null && File.Exists(FilePath);

        private Uri Url => ValidFile ? new Uri($"file:///{FilePath}") : null;

        private FileSystemWatcher Watcher { get; }

        public MainWindow()
        {
            InitializeComponent();

            var argv = Environment.GetCommandLineArgs();
            var filePath = argv.Length == 2 ? argv[1] : null;
            FilePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(Environment.CurrentDirectory, filePath);

            if (FilePath != null)
            {
                try
                {
                    Watcher = new FileSystemWatcher(
                        Path.GetDirectoryName(FilePath),
                        Path.GetFileName(FilePath));
                    Watcher.Changed += FileChangedHandler;
                    Watcher.Deleted += FileDeletedHandler;
                    Watcher.Created += FileCreatedHandler;
                    Watcher.EnableRaisingEvents = true;
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(this, $"Failed to watch the given path: {FilePath}", Title, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Display(Uri url) => webBrowser.Navigate(url ?? new Uri("about:blank"));

        private void UpdateDisplay() => Display(Url);

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void FileCreatedHandler(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("FileSystemWatcher: File Created");
            Dispatcher.BeginInvoke((Action)UpdateDisplay);
        }

        private void FileChangedHandler(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("FileSystemWatcher: File Changed");
            Dispatcher.BeginInvoke((Action<bool>)webBrowser.Refresh, false);
        }

        private void FileDeletedHandler(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("FileSystemWatcher: File Deleted");
            // UpdateDisplay();
        }

        private void WebBrowserLoadCompletedHandler(object sender, NavigationEventArgs e)
        {
            Title = ((dynamic)webBrowser.Document).Title;
        }
    }
}
