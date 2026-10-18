using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mercury.Desktop.Converters;

/// <summary>
/// Turns a plain key from a view model (for example "Store") into the matching resource
/// (the geometry <c>Icon.Store</c>), so view models never need to reference WPF resources.
/// Set <see cref="Prefix"/> to the part of the resource name that comes before the key.
/// </summary>
public sealed class ResourceKeyConverter : IValueConverter
{
    public string Prefix { get; set; } = string.Empty;

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string key ? Application.Current.TryFindResource(Prefix + key) : null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
