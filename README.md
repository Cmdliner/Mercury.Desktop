# Mercury Desktop

The Windows app for Mercury (WPF, .NET 10). It uses the same design system as the Mercury website.

## Run

```
dotnet run
```

Debug builds also have developer switches:

```
dotnet run -- --design-system        # opens the style gallery
dotnet run -- --dashboard [--maximize]  # opens the app already past onboarding
dotnet run -- --shots <folder>       # saves a PNG of each onboarding step, the dashboard (open and collapsed) and the gallery
```

The style gallery (`Views/DesignSystem`) shows every colour, text style and control. Open it before
building a screen, and reuse what is there.

## Structure

```
Resources/
  Theme.xaml            App.xaml merges only this file
  Tokens/               the raw values of the design system
    Colors.xaml           Brush.* and Color.*
    Typography.xaml       Font.* and FontSize.*
    Metrics.xaml          Radius.*, Size.*, Space.*, Padding.*
    Icons.xaml            Icon.* geometries (24 x 24 grid)
  Styles/               how each control looks, built only from tokens
    Text.xaml  Buttons.xaml  Inputs.xaml  Choices.xaml  Surfaces.xaml  Brand.xaml  Windows.xaml
    Chrome.xaml           title-bar caption buttons
    Navigation.xaml       sidebar navigation list
    Scrollbars.xaml       the slim scroll bar (implicit, applies everywhere)
    Converters.xaml       value converters registered as Converter.*
  Fonts/                Geist, embedded (SIL Open Font License)
Controls/               small reusable pieces: Icon, LogoMark, FormField, InputHelper, PasswordBoxHelper, WindowControls
Converters/             Naira, Scale, ResourceKey (icon lookup), BoolToVisibility and its inverse
Models/                 plain data (DemoData holds the sample figures the pages show for now)
Services/               app-wide state, such as PrivacySettings
ViewModels/             logic, one folder per feature: ShellViewModel, Login/, Onboarding/, Workspace/
Views/                  XAML, one folder per feature: Login/, Onboarding/, Workspace/, DesignSystem/
Dev/                    Debug-only tools (gallery launcher, screenshot runner)
```

## Using the design system

- **Colour:** use `{StaticResource Brush.Foreground}` and friends. Never write a hex value in a view.
- **Text:** `Style="{StaticResource Text.Title}"` (also Display, Subtitle, Heading, Lead, Body, Muted, Label, Caption, Eyebrow, Error).
- **Buttons:** every `Button` is Primary. Others: `Button.Outline`, `Button.Secondary`, `Button.Ghost`, `Button.Icon`. Add `.Lg` for the main action of a screen (`Button.Primary.Lg`).
- **Inputs:** `TextBox`, `PasswordBox`, `ComboBox` and `CheckBox` are styled by default. Wrap an input in `controls:FormField` for a label and its validation message. Add a hint with `controls:InputHelper.Placeholder`. Bind a password with `controls:PasswordBoxHelper.BoundPassword`.
- **Surfaces:** `Surface.Card`, `Surface.Divider`, `Pill.Success`, `Pill.Caution`. Depth comes from tone and a 1px border, never shadows.
- **Shape:** controls are square. Containers use `Radius.Xl`, `Radius.2xl` or `Radius.3xl`.
- **New windows:** add `Style="{StaticResource {x:Type Window}}"`. WPF does not apply implicit styles to classes derived from `Window`.
- **Sidebar navigation:** `ListBox.Navigation` with `NavItemViewModel` items (title, icon key, section). It follows `IsSidebarCollapsed` and shows icons only when collapsed.
- **Scroll bars:** nothing to apply. Every `ScrollViewer` gets the slim style.
- **Stat figures:** `Text.Stat` with `Converter.Naira` for money; `Surface.Avatar` for initials.
- **Fonts:** each Geist weight is its own family (`Font.Regular`, `Font.Medium`, `Font.SemiBold`, `Font.Mono`). Pick a weight with the family, not `FontWeight`.

## Adding a style file

A style dictionary can only `StaticResource` things that are in itself or in a dictionary it merges, not in a
sibling. So every file under `Styles/` merges `Tokens/Tokens.xaml` (and any style file it uses) at the top.

## Validation

View models derive from `ObservableValidator` (CommunityToolkit.Mvvm) and use data annotations
(`[Required]`, `[EmailAddress]`, `[Compare]`). `FormField` shows the message on its own.

## Onboarding

Four steps: Business details, Owner account, First store, Finish. `OnboardingViewModel` owns the steps and
raises `Completed` with an `OnboardingResult`; the matching view for each step is chosen by the DataTemplates in
`OnboardingView.xaml`, so view models never create views. Sending the result to the backend is still to do.

## Window and title bar

`MainWindow` has no system frame (`WindowStyle=None` with `WindowChrome`). The top 64px is the drag area and the
edges resize. `Controls/WindowControls` draws minimise, maximise/restore and close. Anything clickable inside the
drag area must set `WindowChrome.IsHitTestVisibleInChrome="True"`, or the drag area swallows the click. The
window is capped to the screen's work area, and padded back in when maximised.

## Shell and workspace

`ShellViewModel` holds `Current`: the sign-in screen at first, then the workspace after signing in, or onboarding for
"Create an account" and then the workspace once it completes. Sign-in only validates the form for now; there is no
backend check yet.
`MainWindow` picks the view for each with DataTemplates.

`WorkspaceViewModel` is built from the onboarding result. It has the sidebar items, the selected page and
`IsSidebarCollapsed` (toggled from the top bar; the sidebar animates between 260 and 76 wide). Each page is a
`PageViewModel` with a matching view in `Views/Workspace`: `OverviewView` is built (stat cards, sales chart,
recent transactions, reconciliation), and so are `StoresView` (a card per store plus an "Add store" dialog) and
`PaymentsView` (the account number a customer pays into for the chosen store, with a Copy button). Stores and
Payments share one list of `StoreAccount`s, so a new store appears in both. `TransactionsView` filters by store, type and status and opens a detail drawer
with the ledger entries; `AccountsView` shows the accounts with their share of the total and recent ledger entries;
`StaffView` lists the team with an "Add staff" dialog. `BeadIllustration` is the line-art used for empty states.

### Hiding amounts

The eye button in the top bar masks every money figure (`₦ ••••••`) so nobody reading over your shoulder sees them.
Show an amount with `<controls:Money Amount="{Binding Balance}" />`, never a plain TextBlock with a converter, or
it will ignore the switch. The switch is `Services/PrivacySettings`; it resets to visible each time the workspace opens. The overview uses
sample data from `DemoData` until the backend exists.
