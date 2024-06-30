using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using TlkLocalisationTool.Logic.DependencyInjection;
using TlkLocalisationTool.Shared.Settings;
using TlkLocalisationTool.UI.Constants;
using TlkLocalisationTool.UI.DependencyInjection;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;
using TlkLocalisationTool.UI.ViewModels;

namespace TlkLocalisationTool.UI;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        Current.DispatcherUnhandledException += OnUnhandledException;

        var appSettings = ReadAppSettings();
        ServiceProviderContainer.TrySetServiceProvider(CreateServiceProvider(appSettings));

        var viewerViewModel = ServiceProviderContainer.GetRequiredService<TlkViewerViewModel>();
        Dialog.Show(viewerViewModel, null);
    }

    private static ServiceProvider CreateServiceProvider(AppSettings appSettings)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(appSettings);
        serviceCollection.AddServices();
        serviceCollection.AddUI();
        return serviceCollection.BuildServiceProvider();
    }

    private static AppSettings ReadAppSettings()
    {
        var appSettingsJson = File.ReadAllText(DataConstants.AppSettingsFileName);
        var appSettings = JsonSerializer.Deserialize<AppSettings>(appSettingsJson);
        return appSettings;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, Strings.ErrorMessage_Title);
        e.Handled = true;
    }
}
