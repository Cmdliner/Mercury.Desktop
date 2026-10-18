using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Base class for a page shown inside the workspace (Overview, Stores and so on).</summary>
public abstract class PageViewModel : ObservableObject
{
    /// <summary>The page's heading, shown in the top bar.</summary>
    public abstract string Title { get; }
}
