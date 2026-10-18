using System.Windows;
using System.Windows.Controls;

namespace Mercury.Desktop.Controls;

/// <summary>
/// The Mercury mark: a bead cut on the diagonal with the halves slid apart. It is drawn
/// in <see cref="Control.Foreground"/>, so use the foreground colour on light surfaces and
/// white on dark ones. Keep it flat; never add effects or an accent colour.
/// </summary>
public class LogoMark : Control
{
    static LogoMark()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LogoMark), new FrameworkPropertyMetadata(typeof(LogoMark)));
    }
}
