using System;
using System.Windows;
using Carhartt.Core;

namespace Carhartt.App
{
    public partial class MainWindow : Window
    {
        private BrowserViewModel? _viewModel;
        private readonly string _userDataFolder;
        private readonly string _initialUrl;

        public MainWindow(string userDataFolder, string initialUrl)
        {
            InitializeComponent();
            _userDataFolder = userDataFolder;
            _initialUrl = initialUrl;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Init ViewModel
                _viewModel = new BrowserViewModel(browserView);
                DataContext = _viewModel;

                Logger.Log($"Initializing WebView2 with User Data Folder: {_userDataFolder}");
                await browserView.InitializeAsync(_userDataFolder);
                
                Logger.Log($"Navigating to initial URL: {_initialUrl}");
                _viewModel.AddressBarUrl = _initialUrl;
                _viewModel.NavigateCommand.Execute(null);
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