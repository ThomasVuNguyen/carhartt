using System;
using System.IO;
using System.Windows;
using Carhartt.Core;
using Carhartt.App.Engine;

namespace Carhartt.App
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private readonly string _userDataFolder;
        private readonly string _initialUrl;

        public MainWindow(string userDataFolder, string initialUrl)
        {
            InitializeComponent();
            _userDataFolder = userDataFolder;
            _initialUrl = initialUrl;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Core factory function to create platform-specific views
                Func<IWebEngineView> viewFactory = () =>
                {
                    var adapter = new WebView2Adapter();
                    // Initialize immediately. 
                    // Note: In a production app you might want to await this, 
                    // but since the View is being created for the VM, 
                    // we can let the async task run.
                    // We catch exceptions inside InitializeAsync's continuation if needed, 
                    // but here we just fire it.
                    _ = adapter.InitializeAsync(_userDataFolder);
                    return adapter;
                };

                _viewModel = new MainViewModel(viewFactory);
                DataContext = _viewModel;

                Logger.Log($"Initializing with User Data Folder: {_userDataFolder}");
                
                // Add initial tab
                Logger.Log($"Adding initial tab: {_initialUrl}");
                _ = _viewModel.AddTab(_initialUrl);
            }
            catch (Exception ex)
            {
                string msg = $"Initialization failed: {ex.Message}";
                Logger.Log(msg);
                MessageBox.Show(msg, "Carhartt Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}