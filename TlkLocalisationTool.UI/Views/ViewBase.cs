using System.Windows;
using System.Windows.Controls;
using TlkLocalisationTool.UI.ViewModels;

namespace TlkLocalisationTool.UI.Views;

public abstract class ViewBase : Page
{
    private ViewModelBase _viewModel;

    public ViewBase()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public void SetViewModel(ViewModelBase viewModel)
    {
        viewModel.ClosureRequested += ((Window)Parent).Close;
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.Init();
        ((Window)Parent).Title = _viewModel.Title;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.ClosureRequested -= ((Window)Parent).Close;
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }
}
