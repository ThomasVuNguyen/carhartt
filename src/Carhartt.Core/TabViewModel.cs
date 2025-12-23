using System;

namespace Carhartt.Core
{
    public class TabViewModel : ViewModelBase
    {
        private readonly IWebEngineView _view;
        private string _addressBarUrl = "";
        private string _title = "New Tab";
        private bool _isLoading;

        public TabViewModel(IWebEngineView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            
            _view.UrlChanged += (s, url) => { AddressBarUrl = url; };
            _view.TitleChanged += (s, title) => { Title = title; };
            _view.LoadingStateChanged += (s, loading) => { IsLoading = loading; };

            NavigateCommand = new RelayCommand(o => Navigate());
            BackCommand = new RelayCommand(o => _view.GoBack());
            ForwardCommand = new RelayCommand(o => _view.GoForward());
            ReloadCommand = new RelayCommand(o => _view.Reload());
            StopCommand = new RelayCommand(o => _view.Stop());
            DevToolsCommand = new RelayCommand(o => _view.OpenDevTools());
        }

        public IWebEngineView View => _view; // Expose View for getting PIDs

        public string AddressBarUrl
        {
            get => _addressBarUrl;
            set => SetProperty(ref _addressBarUrl, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand NavigateCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand ForwardCommand { get; }
        public RelayCommand ReloadCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand DevToolsCommand { get; }

        public void Navigate(string? urlOverride = null)
        {
            string targetUrl = urlOverride ?? AddressBarUrl;
            if (!string.IsNullOrWhiteSpace(targetUrl))
            {
                if (!targetUrl.StartsWith("http://") && !targetUrl.StartsWith("https://"))
                {
                    targetUrl = "https://" + targetUrl;
                }
                _view.Navigate(targetUrl);
            }
        }
    }
}
