using System.Windows;

namespace Mercury.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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
