using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.Extensions;

internal static class ListViewExtensions
{
    public static readonly DependencyProperty DataSetProperty = DependencyProperty.RegisterAttached("DataSet", typeof(TableDataSet), typeof(ListViewExtensions), new PropertyMetadata(OnDataSetChanged));

    public static TableDataSet GetDataSet(DependencyObject dependencyObject)
    {
        return (TableDataSet)dependencyObject.GetValue(DataSetProperty);
    }

    public static void SetDataSet(DependencyObject dependencyObject, TableDataSet value)
    {
        dependencyObject.SetValue(DataSetProperty, value);
    }

    private static void OnDataSetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        var listView = dependencyObject as ListView;
        var gridView = listView.View as GridView;
        gridView.Columns.Clear();
        var dataSet = GetDataSet(listView);
        listView.ItemsSource = dataSet.Rows;
        for (var i = 0; i < dataSet.ColumnNames.Length; i++)
        {
            var gridViewColumn = new GridViewColumn { Header = dataSet.ColumnNames[i], DisplayMemberBinding = new Binding($"{nameof(TableRow.Entries)}[{i}]") };
            gridView.Columns.Add(gridViewColumn);
        }
    }
}
