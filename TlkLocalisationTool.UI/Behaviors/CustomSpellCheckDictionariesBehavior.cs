using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TlkLocalisationTool.UI.Behaviors;

internal class CustomSpellCheckDictionariesBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty DictionaryUrisProperty = DependencyProperty.Register(nameof(DictionaryUris), typeof(IEnumerable<Uri>), typeof(CustomSpellCheckDictionariesBehavior));

    public IEnumerable<Uri> DictionaryUris
    {
        get => (IEnumerable<Uri>)GetValue(DictionaryUrisProperty);
        set => SetValue(DictionaryUrisProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.Loaded -= OnLoaded;
        foreach (var dictionaryUri in DictionaryUris)
        {
            AssociatedObject.SpellCheck.CustomDictionaries.Add(dictionaryUri);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.SpellCheck.CustomDictionaries.Clear();
        AssociatedObject.Unloaded -= OnUnloaded;
    }
}
