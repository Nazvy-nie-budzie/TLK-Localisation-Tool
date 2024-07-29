using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TlkLocalisationTool.UI.Behaviors;

internal class TreeViewScrollToSelectionBehavior : Behavior<TreeView>
{
    private const double HorizontalScrollOffset = 100;
    private const double VerticalScrollOffset = 100; 

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectedItemChanged += OnSelectedItemChanged;
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        BringSelectedItemIntoView();
        AssociatedObject.SelectedItemChanged -= OnSelectedItemChanged;
    }

    private void BringSelectedItemIntoView()
    {
        var itemsControlsQueue = new Queue<ItemsControl>([AssociatedObject]);
        do
        {
            var itemsControl = itemsControlsQueue.Dequeue();
            foreach (var item in itemsControl.Items)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is not TreeViewItem treeViewItem)
                {
                    break;
                }

                if (treeViewItem.IsSelected)
                {
                    treeViewItem.Focus();
                    treeViewItem.BringIntoView(new Rect(0, 0, HorizontalScrollOffset, VerticalScrollOffset));
                    return;
                }

                itemsControlsQueue.Enqueue(treeViewItem);
            }
        }
        while (itemsControlsQueue.Count != 0);
    }
}
