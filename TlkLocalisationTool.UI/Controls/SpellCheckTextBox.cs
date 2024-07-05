using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TlkLocalisationTool.UI.Controls;

internal class SpellCheckTextBox : TextBox
{
    static SpellCheckTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpellCheckTextBox), new FrameworkPropertyMetadata(typeof(SpellCheckTextBox)));
    }

    public SpellCheckTextBox()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty SpellCheckDictionariesProperty =
        DependencyProperty.Register("SpellCheckDictionaries", typeof(List<Uri>), typeof(SpellCheckTextBox));

    public static List<Uri> GetSpellCheckDictionaries(SpellCheckTextBox target) => (List<Uri>)target.GetValue(SpellCheckDictionariesProperty);

    public static void SetSpellCheckDictionaries(SpellCheckTextBox target, List<Uri> value) => target.SetValue(SpellCheckDictionariesProperty, value);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var attachedSpellCheckDictionaries = GetSpellCheckDictionaries(this);
        attachedSpellCheckDictionaries.ForEach(x => SpellCheck.CustomDictionaries.Add(x));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        SpellCheck.CustomDictionaries.Clear();
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }
}
