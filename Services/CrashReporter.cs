using System.IO;
using System.Windows;

namespace Mercury.Desktop.Services;

/// <summary>
/// Makes sure a failure is never silent. Without this, an app that cannot start just vanishes and the
/// person is left clicking an icon that does nothing. Every problem is written to a log file, and the
/// person is told in plain words where to find it.
/// </summary>
public static class CrashReporter
{
    public static string LogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mercury", "crash.log");

    // Automated checks set this so a failure is logged without leaving a dialog open.
    private static bool DialogsDisabled => Environment.GetEnvironmentVariable("MERCURY_NO_DIALOGS") == "1";

    public static void Log(Exception exception, string where)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {where}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // If even the log cannot be written there is nothing more to do with it.
        }
    }

    /// <summary>The app could not start. Says so, and where the details are.</summary>
    public static void ShowFatal(Exception exception)
    {
        Log(exception, "The app could not start");
        if (DialogsDisabled) return;

        MessageBox.Show(
            $"Mercury could not start.\n\n{exception.GetBaseException().Message}\n\nThe details were saved to:\n{LogPath}\n\nIf this keeps happening, send that file to support.",
            "Mercury could not start",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>Something failed while the app was running. The app carries on.</summary>
    public static void ShowRecoverable(Exception exception)
    {
        Log(exception, "An error while the app was running");
        if (DialogsDisabled) return;

        MessageBox.Show(
            $"Something went wrong.\n\n{exception.GetBaseException().Message}\n\nThe details were saved to:\n{LogPath}",
            "Mercury",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
