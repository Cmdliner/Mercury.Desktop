using System.Windows;
using System.Windows.Controls;

namespace Mercury.Desktop.Controls;

/// <summary>
/// WPF's PasswordBox.Password cannot be data-bound. This helper adds a bindable
/// <see cref="BoundPasswordProperty"/> and a read-only <see cref="HasPasswordProperty"/>
/// (used to show the placeholder), without exposing the password anywhere else.
/// </summary>
public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundPasswordChanged));

    private static readonly DependencyPropertyKey HasPasswordPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "HasPassword",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasPasswordProperty =
        HasPasswordPropertyKey.DependencyProperty;

    private static bool _isUpdatingFromBox;

    static PasswordBoxHelper()
    {
        // One class-level handler covers every PasswordBox in the app.
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            new RoutedEventHandler(OnPasswordChanged));
    }

    public static string GetBoundPassword(DependencyObject element) =>
        (string)element.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject element, string value) =>
        element.SetValue(BoundPasswordProperty, value);

    public static bool GetHasPassword(DependencyObject element) =>
        (bool)element.GetValue(HasPasswordProperty);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox box || _isUpdatingFromBox) return;

        var password = e.NewValue as string ?? string.Empty;
        if (box.Password != password) box.Password = password;
        box.SetValue(HasPasswordPropertyKey, password.Length > 0);
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox box) return;

        box.SetValue(HasPasswordPropertyKey, box.Password.Length > 0);

        _isUpdatingFromBox = true;
        try
        {
            SetBoundPassword(box, box.Password);
        }
        finally
        {
            _isUpdatingFromBox = false;
        }
    }
}
