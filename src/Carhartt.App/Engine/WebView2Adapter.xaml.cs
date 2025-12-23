using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Carhartt.Core;
using Microsoft.Web.WebView2.Core;

namespace Carhartt.App.Engine
{
    public partial class WebView2Adapter : UserControl, IWebEngineView
    {
        public event EventHandler<string>? UrlChanged;
        public event EventHandler<string>? TitleChanged;
        public event EventHandler<bool>? LoadingStateChanged;

        public WebView2Adapter()
        {
            InitializeComponent();
            webView.NavigationStarting += (s, e) => LoadingStateChanged?.Invoke(this, true);
            webView.NavigationCompleted += (s, e) => 
            {
                LoadingStateChanged?.Invoke(this, false);
                if (!e.IsSuccess && e.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
                {
                    string errorHtml = $"<html><body style='font-family: sans-serif; padding: 20px'><h1>Navigation Failed</h1><p>Error Status: {e.WebErrorStatus}</p></body></html>";
                    webView.NavigateToString(errorHtml);
                }
            };
            webView.SourceChanged += (s, e) => UrlChanged?.Invoke(this, webView.Source.ToString());
            webView.ContentLoading += (s, e) => { }; // Optional
        }

        public async Task InitializeAsync(string userDataFolder)
        {
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView.EnsureCoreWebView2Async(env);

            // Bind TitleChanged after CoreWebView2 is initialized
            webView.CoreWebView2.DocumentTitleChanged += (s, e) => 
                TitleChanged?.Invoke(this, webView.CoreWebView2.DocumentTitle);
        }

        public void Navigate(string url)
        {
            if (webView.CoreWebView2 != null && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                webView.CoreWebView2.Navigate(uri.ToString());
            }
        }

        public void GoBack()
        {
            if (webView.CanGoBack) webView.GoBack();
        }

        public void GoForward()
        {
            if (webView.CanGoForward) webView.GoForward();
        }

        public void Reload()
        {
            webView.Reload();
        }

        public void Stop()
        {
            webView.Stop();
        }

        public void OpenDevTools()
        {
            webView.CoreWebView2?.OpenDevToolsWindow();
        }
    }
}
