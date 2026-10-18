using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mercury.Desktop.Controls;

/// <summary>
/// A stroked outline icon. Pick a geometry from Resources/Tokens/Icons.xaml; the icon
/// takes its colour from <see cref="Control.Foreground"/> and scales with Width/Height.
/// </summary>
public class Icon : Control
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(Icon));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(Icon), new PropertyMetadata(2.0));

    static Icon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));
    }

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
}
