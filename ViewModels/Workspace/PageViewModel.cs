using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Base class for a page shown inside the workspace (Overview, Stores and so on).</summary>
public abstract class PageViewModel : ObservableObject
{
    /// <summary>The page's heading, shown in the top bar.</summary>
    public abstract string Title { get; }

    /// <summary>
    /// True while the page still shows built-in sample figures instead of the business's own, so
    /// the app can say so rather than let sample numbers pass for real ones.
    /// </summary>
    public bool IsSampleData { get; init; }
}
