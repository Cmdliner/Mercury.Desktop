using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Mercury.Desktop.Services;

namespace Mercury.Desktop.Controls;

/// <summary>
/// An amount in naira that respects the app's "hide amounts" switch. Use it instead of a
/// TextBlock wherever a figure is shown: <c>&lt;controls:Money Amount="{Binding Balance}" /&gt;</c>.
/// It takes any TextBlock style, so it can be sized and coloured like other text.
/// </summary>
public sealed class Money : TextBlock
{
    private static readonly CultureInfo Culture = new("en-NG");
    private const string Mask = "₦ ••••••";

    public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
        nameof(Amount), typeof(decimal), typeof(Money),
        new PropertyMetadata(0m, (d, _) => ((Money)d).Refresh()));

    public Money()
    {
        Loaded += (_, _) =>
        {
            PrivacySettings.Instance.PropertyChanged += OnPrivacyChanged;
            Refresh();
        };
        Unloaded += (_, _) => PrivacySettings.Instance.PropertyChanged -= OnPrivacyChanged;
    }

    public decimal Amount
    {
        get => (decimal)GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }

    private void OnPrivacyChanged(object? sender, PropertyChangedEventArgs e) => Refresh();

    private void Refresh() =>
        Text = PrivacySettings.Instance.AmountsHidden ? Mask : Amount.ToString("C0", Culture);
}
