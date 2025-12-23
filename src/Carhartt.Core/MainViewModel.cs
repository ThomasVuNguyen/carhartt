using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Carhartt.Core
{
    public class MainViewModel : ViewModelBase
    {
        private TabViewModel? _selectedTab;
        private string _memoryUsageMb = "0 MB";
        private string _cpuUsagePercent = "0%";
        private readonly Func<IWebEngineView> _viewFactory;
        private readonly MetricsService _metricsService = new MetricsService();
        private bool _isMetricsRunning;

        public MainViewModel(Func<IWebEngineView> viewFactory)
        {
            _viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            Tabs = new ObservableCollection<TabViewModel>();
            
            AddTabCommand = new RelayCommand(o => AddTab());
            CloseTabCommand = new RelayCommand(o => CloseTab(o as TabViewModel));
            
            // Start metrics loop (Simulated timer for Core, in real app use DispatcherTimer in UI or Task.Delay)
            StartMetricsLoop();
        }

        public ObservableCollection<TabViewModel> Tabs { get; }

        public TabViewModel? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public string MemoryUsageMb
        {
            get => _memoryUsageMb;
            set => SetProperty(ref _memoryUsageMb, value);
        }

        public string CpuUsagePercent
        {
            get => _cpuUsagePercent;
            set => SetProperty(ref _cpuUsagePercent, value);
        }

        public RelayCommand AddTabCommand { get; }
        public RelayCommand CloseTabCommand { get; }

        public async Task AddTab(string? initialUrl = null)
        {
            var view = _viewFactory(); 
            // Note: InitializeAsync needs to be called by the UI mechanism or Factory. 
            // In WPF, the View is a generic Control.
            // We'll rely on the UI binding to initialize the view when it's rendered, 
            // OR the Factory returns an initialized wrapper.
            // Simplified approach: The View is created here, but InitializeAsync must be handled.
            // We'll let the UI handle InitializeAsync when the Control is Loaded.
            
            var tab = new TabViewModel(view);
            Tabs.Add(tab);
            SelectedTab = tab;
            
            if (initialUrl != null)
            {
                // We might need to wait for initialization. 
                // Since this is generic Core, we rely on the View handling Navigate calls queueing or UI handling it.
                // For MVP, we pass it to the VM which calls Navigate.
                tab.Navigate(initialUrl);
            }
        }

        private void CloseTab(TabViewModel? tab)
        {
            if (tab == null) return;
            Tabs.Remove(tab);
            tab.View.Stop(); // Cleanup
            if (Tabs.Count == 0)
            {
                // If last tab, maybe close app? For now, add a new blank tab.
                AddTab();
            }
            else if (SelectedTab == tab || SelectedTab == null)
            {
                SelectedTab = Tabs.LastOrDefault();
            }
        }

        private async void StartMetricsLoop()
        {
            if (_isMetricsRunning) return;
            _isMetricsRunning = true;

            while (_isMetricsRunning)
            {
                try
                {
                    await UpdateMetrics();
                    await Task.Delay(1000);
                }
                catch
                {
                    await Task.Delay(5000);
                }
            }
        }

        private async Task UpdateMetrics()
        {
            var allPids = new List<int>();
            foreach (var tab in Tabs.ToList()) // ToList to avoid concurrent mod
            {
                try
                {
                    var pids = await tab.View.GetProcessIds();
                    allPids.AddRange(pids);
                }
                catch { }
            }

            var metrics = _metricsService.GetMetrics(allPids);
            MemoryUsageMb = $"{metrics.MemoryMb:F1} MB";
            CpuUsagePercent = $"{metrics.CpuPercent:F1}%";
        }
    }
}
