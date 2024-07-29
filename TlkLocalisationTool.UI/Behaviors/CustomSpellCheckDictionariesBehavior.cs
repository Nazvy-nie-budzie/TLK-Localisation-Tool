using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Controls;

namespace TlkLocalisationTool.UI.Behaviors;

internal class CustomSpellCheckDictionariesBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty DictionaryUrisProperty =
        DependencyProperty.Register("DictionaryUris", typeof(List<Uri>), typeof(CustomSpellCheckDictionariesBehavior));

    public static List<Uri> GetDictionaryUris(CustomSpellCheckDictionariesBehavior target) => (List<Uri>)target.GetValue(DictionaryUrisProperty);

    public static void SetDictionaryUris(CustomSpellCheckDictionariesBehavior target, List<Uri> value) => target.SetValue(DictionaryUrisProperty, value);

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var attachedDictionaryUris = GetDictionaryUris(this);
        attachedDictionaryUris.ForEach(x => AssociatedObject.SpellCheck.CustomDictionaries.Add(x));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.SpellCheck.CustomDictionaries.Clear();
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.Unloaded -= OnUnloaded;
    }
}
