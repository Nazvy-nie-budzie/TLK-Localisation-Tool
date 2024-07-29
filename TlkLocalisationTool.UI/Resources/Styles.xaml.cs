using System.Windows;
using System.Windows.Controls;

namespace TlkLocalisationTool.UI.Resources;

public partial class Styles : ResourceDictionary
{
    public Styles()
    {
        InitializeComponent();
    }

    private void OnTreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem)
        {
            e.Handled = true;
        }
    }
}
