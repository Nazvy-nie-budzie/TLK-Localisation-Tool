using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Threading;
using TlkLocalisationTool.Logic.DependencyInjection;
using TlkLocalisationTool.Logic.Services.Interfaces;
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

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddJsonServices();
        var tempServiceProvider = serviceCollection.BuildServiceProvider();

        var appSettings = ReadAppSettings(tempServiceProvider);
        var serviceProvider = CreateServiceProvider(serviceCollection, appSettings);
        ServiceProviderContainer.TrySetServiceProvider(serviceProvider);

        var viewerViewModel = ServiceProviderContainer.GetRequiredService<TlkViewerViewModel>();
        Dialog.Show(viewerViewModel, null);
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        if (e.ApplicationExitCode != 0)
        {
            return;
        }

        var appSettings = ServiceProviderContainer.GetRequiredService<AppSettings>();
        var jsonWriter = ServiceProviderContainer.GetRequiredService<IJsonWriter>();
        jsonWriter.WriteSync(appSettings, DataConstants.AppSettingsFileName);
    }

    private static ServiceProvider CreateServiceProvider(ServiceCollection serviceCollection, AppSettings appSettings)
    {
        serviceCollection.AddSingleton(appSettings);
        serviceCollection.AddServices();
        serviceCollection.AddUI();
        return serviceCollection.BuildServiceProvider();
    }

    private static AppSettings ReadAppSettings(ServiceProvider serviceProvider)
    {
        var jsonReader = serviceProvider.GetRequiredService<IJsonReader>();
        var appSettings = jsonReader.ReadSync<AppSettings>(DataConstants.AppSettingsFileName);
        return appSettings;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, Strings.ErrorMessage_Title);
        e.Handled = true;

        if (Windows.Count == 0)
        {
            Shutdown(1);
        }
    }
}
