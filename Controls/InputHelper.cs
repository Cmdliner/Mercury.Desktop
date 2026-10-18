using System.Windows;

namespace Mercury.Desktop.Controls;

/// <summary>
/// Attached properties shared by Mercury's text-style inputs.
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Hint shown inside an empty TextBox, PasswordBox or ComboBox:
    /// <c>&lt;TextBox controls:InputHelper.Placeholder="Business name" /&gt;</c>.
    /// </summary>
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(InputHelper),
            new PropertyMetadata(string.Empty));

    public static string GetPlaceholder(DependencyObject element) =>
        (string)element.GetValue(PlaceholderProperty);

    public static void SetPlaceholder(DependencyObject element, string value) =>
        element.SetValue(PlaceholderProperty, value);
}
