namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>One entry in the sidebar: its label, icon, section heading and the page it opens.</summary>
/// <param name="Title">The label, for example "Overview".</param>
/// <param name="IconKey">The icon's name in Resources/Tokens/Icons.xaml without the "Icon." prefix, for example "Grid".</param>
/// <param name="Section">The group heading it appears under, for example "OPERATIONS".</param>
/// <param name="Page">The page it opens.</param>
public sealed record NavItemViewModel(string Title, string IconKey, string Section, PageViewModel Page);
