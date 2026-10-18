using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.Views.DesignSystem;

/// <summary>One colour token shown in the gallery.</summary>
public sealed record Swatch(string Name, Brush Brush, string Hex);

/// <summary>
/// The style gallery: every token and control of the design system on one page. Colours are
/// read from the live theme, so this window can never disagree with the app.
/// </summary>
public partial class DesignSystemWindow : Window
{
    private static readonly string[] BrushNames =
    [
        "Background", "SurfaceMuted", "Card",
        "Foreground", "MutedForeground", "SubtleForeground",
        "Border", "BorderStrong", "Input", "Ring",
        "Primary", "PrimaryHover", "Secondary", "SecondaryHover",
        "Success", "SuccessSoft", "Sun", "SunSoft", "Destructive",
        "Sidebar", "SidebarMuted", "SidebarStep",
    ];

    public DesignSystemWindow()
    {
        InitializeComponent();
        DataContext = new DesignSystemModel(BuildSwatches());
    }

    /// <summary>The content to capture when taking a full-length screenshot.</summary>
    internal FrameworkElement Capturable => PageRoot;

    private static IReadOnlyList<Swatch> BuildSwatches() =>
        BrushNames
            .Select(name =>
            {
                var brush = (SolidColorBrush)Application.Current.FindResource($"Brush.{name}");
                var color = brush.Color;
                return new Swatch(name, brush, $"#{color.R:X2}{color.G:X2}{color.B:X2}");
            })
            .ToList();
}

/// <summary>Data for the gallery window.</summary>
public sealed class DesignSystemModel(IReadOnlyList<Swatch> swatches)
{
    public IReadOnlyList<Swatch> Swatches { get; } = swatches;

    public ErrorDemoModel Form { get; } = new();
}

/// <summary>A tiny validated model so the gallery can show an input in its error state.</summary>
public sealed partial class ErrorDemoModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    private string _email = "not-an-email";

    public ErrorDemoModel() => ValidateAllProperties();
}
