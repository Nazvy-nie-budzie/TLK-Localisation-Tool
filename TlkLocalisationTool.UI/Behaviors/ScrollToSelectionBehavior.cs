using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TlkLocalisationTool.UI.Behaviors;

internal class ScrollToSelectionBehavior : Behavior<ListView>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject.SelectedItem != null)
        {
            AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
        }
        else
        {
            GetScrollViewer().ScrollToTop();
        }
    }

    private ScrollViewer GetScrollViewer()
    {
        var dependencyObjectsQueue = new Queue<DependencyObject>([AssociatedObject]);
        while (dependencyObjectsQueue.Count != 0)
        {
            var dependencyObject = dependencyObjectsQueue.Dequeue();
            var dependencyObjectChildrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var i = 0; i < dependencyObjectChildrenCount; i++)
            {
                var dependencyObjectChild = VisualTreeHelper.GetChild(dependencyObject, i);
                if (dependencyObjectChild is ScrollViewer scrollViewer)
                {
                    return scrollViewer;
                }

                dependencyObjectsQueue.Enqueue(dependencyObjectChild);
            }
        }

        return null;
    }
}
