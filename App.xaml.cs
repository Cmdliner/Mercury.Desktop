using System.Windows;
using System.Windows.Controls;

namespace Mercury.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // WPF draws its own red outline around any element with a validation error, and when the
        // error belongs to a bound container the outline spreads to the whole window. The app
        // shows errors itself (red field border plus the message under it), so turn the built-in
        // outline off everywhere.
        Validation.ErrorTemplateProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(new ControlTemplate()));

#if DEBUG
        System.Diagnostics.PresentationTraceSources.Refresh();
        System.Diagnostics.PresentationTraceSources.DataBindingSource.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;

        // Developer switches: --design-system opens the style gallery, --shots <folder> saves PNGs.
        if (Dev.DevTools.TryHandle(e.Args, this)) return;
#endif

        new MainWindow().Show();
    }
}
