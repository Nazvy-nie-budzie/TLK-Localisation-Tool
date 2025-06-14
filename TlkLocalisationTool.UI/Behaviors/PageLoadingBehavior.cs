using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TlkLocalisationTool.UI.Converters;
using TlkLocalisationTool.UI.ViewModels;

namespace TlkLocalisationTool.UI.Behaviors;

internal class PageLoadingBehavior : Behavior<Page>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Initialized += OnInitialised;
    }

    private void OnInitialised(object sender, System.EventArgs e)
    {
        AssociatedObject.Initialized -= OnInitialised;

        var pageContent = AssociatedObject.Content as FrameworkElement;
        var isEnabledBinding = new Binding(nameof(ViewModelBase.IsLoading)) { Converter = new BoolReverseConverter() };
        pageContent.SetBinding(UIElement.IsEnabledProperty, isEnabledBinding);

        var progressBar = new ProgressBar { Width = 200, Height = 20, IsIndeterminate = true, };
        var visibilityBinding = new Binding(nameof(ViewModelBase.IsLoading)) { Converter = new BooleanToVisibilityConverter() };
        progressBar.SetBinding(UIElement.VisibilityProperty, visibilityBinding);

        AssociatedObject.Content = null;

        var grid = new Grid();
        grid.Children.Add(pageContent);
        grid.Children.Add(progressBar);

        AssociatedObject.Content = grid;
    }
}
