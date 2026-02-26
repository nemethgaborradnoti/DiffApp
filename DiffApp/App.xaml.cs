using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiffApp
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<IComparisonService, ComparisonService>();
                    services.AddSingleton<IMergeService, MergeService>();
                    services.AddSingleton<IScrollService, ScrollService>();
                    services.AddSingleton<IHistoryService, HistoryService>();

                    services.AddSingleton<InputViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    services.AddTransient<HistoryViewModel>();

                    services.AddSingleton<EditorViewModel>();

                    services.AddTransient<MainViewModel>();

                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
            base.OnExit(e);
        }
    }
}