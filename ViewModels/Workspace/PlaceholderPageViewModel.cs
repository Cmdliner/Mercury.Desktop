namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Stands in for a page that is designed but not built yet.</summary>
public sealed class PlaceholderPageViewModel(string title, string description) : PageViewModel
{
    public override string Title { get; } = title;

    public string Description { get; } = description;
}
