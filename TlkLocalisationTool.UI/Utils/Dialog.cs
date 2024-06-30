using System;
using System.Windows;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.ViewModels;
using TlkLocalisationTool.UI.Views;

namespace TlkLocalisationTool.UI.Utils;

internal static class Dialog
{
    private const string ModelTypeNamePart = "Model";

    public static void Show(ViewModelBase viewModel, ViewModelBase ownerViewModel) => CreateWindow(viewModel, ownerViewModel).Show();

    public static void ShowDialog(ViewModelBase viewModel, ViewModelBase ownerViewModel) => CreateWindow(viewModel, ownerViewModel).ShowDialog();

    private static Window CreateWindow(ViewModelBase viewModel, ViewModelBase ownerViewModel)
    {
        var view = GetView(viewModel);
        var window = new Window { Title = Strings.DefaultTitle, Content = view, ShowInTaskbar = ownerViewModel == null, WindowStartupLocation = WindowStartupLocation.CenterScreen, };
        view.SetViewModel(viewModel);
        if (ownerViewModel != null)
        {
            window.Owner = GetOwner(ownerViewModel);
        }

        return window;
    }

    private static ViewBase GetView(ViewModelBase viewModel)
    {
        var viewModelTypeName = viewModel.GetType().FullName;
        var viewTypeName = viewModelTypeName.Replace(ModelTypeNamePart, string.Empty);
        var viewType = Type.GetType(viewTypeName, true);
        var view = (ViewBase)ServiceProviderContainer.GetRequiredService(viewType);
        return view;
    }

    private static Window GetOwner(ViewModelBase ownerViewModel)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window.Content is ViewBase view && view.DataContext == ownerViewModel)
            {
                return window;
            }
        }

        return null;
    }
}
