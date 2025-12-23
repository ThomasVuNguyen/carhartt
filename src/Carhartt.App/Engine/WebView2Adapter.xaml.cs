using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Carhartt.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

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

            // Handle server certificate errors (e.g. self-signed)
            webView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                {
                    webView.CoreWebView2.ServerCertificateErrorDetected += (object? sender, CoreWebView2ServerCertificateErrorDetectedEventArgs args) =>
                    {
                        args.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                    };
                }
            };
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

        public Task<System.Collections.Generic.IEnumerable<int>> GetProcessIds()
        {
            var pids = new System.Collections.Generic.List<int>();
            if (webView.CoreWebView2 != null)
            {
                try
                {
                    uint browserPid = webView.CoreWebView2.BrowserProcessId;
                    pids.Add((int)browserPid);

                    /* TODO: Fix build error with GetProcessInfos
                    var infos = webView.CoreWebView2.GetProcessInfos();
                    foreach (var info in infos)
                    {
                        pids.Add(info.ProcessId);
                    }
                    */
                }
                catch { }
            }
            return Task.FromResult<System.Collections.Generic.IEnumerable<int>>(pids);
        }
    }
}
