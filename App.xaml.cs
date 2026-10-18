using System.Windows;
using System.Windows.Controls;
using Mercury.Desktop.Services;

namespace Mercury.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Nothing may fail silently: a problem is logged and shown, never just a window that never opens.
        DispatcherUnhandledException += (_, args) =>
        {
            if (Windows.Count == 0)
            {
                // Nothing is on screen, so there is nothing to carry on with. Without this the process
                // would stay alive with no window at all.
                CrashReporter.ShowFatal(args.Exception);
                Shutdown(1);
            }
            else
            {
                CrashReporter.ShowRecoverable(args.Exception);
            }

            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception) CrashReporter.Log(exception, "Unhandled exception");
        };
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            CrashReporter.Log(args.Exception, "A background task failed");
            args.SetObserved();
        };

        try
        {
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

            // Opened from an invitation link (mercury://accept-invitation?token=...).
            AppServices.PendingInvitationToken = InvitationLink.FromCommandLine(e.Args);

            new MainWindow().Show();
        }
        catch (Exception ex)
        {
            CrashReporter.ShowFatal(ex);
            Shutdown(1);
        }
    }
}
