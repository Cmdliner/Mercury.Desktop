using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.Services;

/// <summary>
/// Whether money amounts are masked on screen. One switch for the whole app, so someone
/// reading over your shoulder sees dots until you reveal the figures.
/// </summary>
public sealed partial class PrivacySettings : ObservableObject
{
    public static PrivacySettings Instance { get; } = new();

    [ObservableProperty]
    private bool _amountsHidden;
}
