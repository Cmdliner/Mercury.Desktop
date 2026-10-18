using System.Globalization;
using System.Windows.Data;

namespace Mercury.Desktop.Converters;

/// <summary>
/// Shows an amount in naira with thousands separators and no decimals, for example
/// <c>₦1,820,400</c>. Use it as <c>{Binding Balance, Converter={StaticResource Converter.Naira}}</c>.
/// </summary>
public sealed class NairaConverter : IValueConverter
{
    private static readonly CultureInfo Culture = new("en-NG");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is decimal amount ? amount.ToString("C0", Culture) : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
