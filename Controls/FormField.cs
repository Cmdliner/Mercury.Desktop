using System.Windows;
using System.Windows.Controls;

namespace Mercury.Desktop.Controls;

/// <summary>
/// A label above an input, with that input's validation message below it.
/// <code>
/// &lt;controls:FormField Header="Business name"&gt;
///     &lt;TextBox Text="{Binding BusinessName, UpdateSourceTrigger=PropertyChanged}" /&gt;
/// &lt;/controls:FormField&gt;
/// </code>
/// The message comes straight from the validation errors on the wrapped input, so a view
/// model only has to implement INotifyDataErrorInfo.
/// </summary>
public class FormField : HeaderedContentControl
{
    static FormField()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormField), new FrameworkPropertyMetadata(typeof(FormField)));
    }
}
