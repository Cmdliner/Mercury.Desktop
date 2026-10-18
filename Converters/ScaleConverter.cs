using System.Globalization;
using System.Windows.Data;

namespace Mercury.Desktop.Converters;

/// <summary>
/// Multiplies a number by the converter parameter. Turns a share (0 to 1) into pixels,
/// for example <c>ConverterParameter=160</c> makes 0.5 into 80.
/// </summary>
public sealed class ScaleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var scale = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
        return value is double share ? share * scale : 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
