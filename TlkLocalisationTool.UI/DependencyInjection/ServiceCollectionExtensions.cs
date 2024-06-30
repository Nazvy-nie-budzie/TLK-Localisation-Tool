using Microsoft.Extensions.DependencyInjection;
using TlkLocalisationTool.UI.ViewModels;
using TlkLocalisationTool.UI.Views;

namespace TlkLocalisationTool.UI.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddUI(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<TlkViewerViewModel>();
        serviceCollection.AddTransient<EntryEditorViewModel>();
        serviceCollection.AddTransient<SettingsEditorViewModel>();
        serviceCollection.AddTransient<ContextSelectorViewModel>();
        serviceCollection.AddTransient<GffViewerViewModel>();
        serviceCollection.AddTransient<TdaViewerViewModel>();
        serviceCollection.AddTransient<DlgViewerViewModel>();

        serviceCollection.AddTransient<TlkViewerView>();
        serviceCollection.AddTransient<EntryEditorView>();
        serviceCollection.AddTransient<SettingsEditorView>();
        serviceCollection.AddTransient<ContextSelectorView>();
        serviceCollection.AddTransient<GffViewerView>();
        serviceCollection.AddTransient<TdaViewerView>();
        serviceCollection.AddTransient<DlgViewerView>();
    }
}
