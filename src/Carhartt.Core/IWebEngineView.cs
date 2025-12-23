using System;
using System.Threading.Tasks;

namespace Carhartt.Core
{
    public interface IWebEngineView
    {
        Task InitializeAsync(string userDataFolder);
        void Navigate(string url);
        void GoBack();
        void GoForward();
        void Reload();
        void Stop();
        void OpenDevTools();
        Task<System.Collections.Generic.IEnumerable<int>> GetProcessIds();

        event EventHandler<string> UrlChanged;
        event EventHandler<string> TitleChanged;
        event EventHandler<bool> LoadingStateChanged;
    }
}
