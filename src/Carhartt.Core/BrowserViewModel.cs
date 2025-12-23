using System;

namespace Carhartt.Core
{
    public class BrowserViewModel : ViewModelBase
    {
        private readonly IWebEngineView _view;
        private string _addressBarUrl = "";
        private string _title = "Carhartt Browser";
        private bool _isLoading;

        public BrowserViewModel(IWebEngineView view)
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

        private void Navigate()
        {
            if (!string.IsNullOrWhiteSpace(AddressBarUrl))
            {
                // Basic normalization
                string url = AddressBarUrl;
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
                _view.Navigate(url);
            }
        }
    }
}
