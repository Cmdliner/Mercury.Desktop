#if DEBUG
using Mercury.Desktop.Models;
using Mercury.Desktop.ViewModels;
using Mercury.Desktop.Views.DesignSystem;

namespace Mercury.Desktop.Dev;

/// <summary>
/// Switches that only exist in Debug builds:
/// <c>dotnet run -- --design-system</c> opens the style gallery, and
/// <c>dotnet run -- --shots &lt;folder&gt;</c> saves PNGs of every onboarding state, and
/// <c>dotnet run -- --dashboard [--maximize]</c> opens the app already past onboarding.
/// </summary>
internal static class DevTools
{
    /// <returns>True when a switch was handled and the normal window should not open.</returns>
    public static bool TryHandle(string[] args, App app)
    {
        if (args.Contains("--design-system"))
        {
            new DesignSystemWindow().Show();
            return true;
        }

        if (args.Contains("--dashboard"))
        {
            var window = new MainWindow();
            ((ShellViewModel)window.DataContext).ShowWorkspace(DemoData.SampleOnboardingResult);
            if (args.Contains("--maximize"))
            {
                window.WindowState = System.Windows.WindowState.Maximized;
            }
            window.Show();
            return true;
        }

        var shots = Array.IndexOf(args, "--shots");
        if (shots >= 0 && shots + 1 < args.Length)
        {
            _ = ScreenshotRunner.RunAsync(args[shots + 1], app);
            return true;
        }

        return false;
    }
}
#endif
