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
  Images/               photos used as textures (login panel, overview banner, store cards, payment panel)
Controls/               small reusable pieces: Icon, LogoMark, FormField, InputHelper, PasswordBoxHelper, WindowControls
Converters/             Naira, Scale, ResourceKey (icon lookup), BoolToVisibility and its inverse
Models/                 plain data (DemoData holds the sample figures the pages show for now)
Services/               app-wide state (PrivacySettings, TokenStore, Permissions) and Api/, the connection to the server
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

Five steps: Business details, Owner account, First store, Payment account, Finish. `OnboardingViewModel` owns the steps and
raises `Completed` with an `OnboardingResult`; the matching view for each step is chosen by the DataTemplates in
`OnboardingView.xaml`, so view models never create views. Sending the result to the backend is still to do.

## Window and title bar

`MainWindow` has no system frame (`WindowStyle=None` with `WindowChrome`). The top 44px is the drag area and the
edges resize. It is a near-black title bar with the brand on the left and the window buttons on the right. `Controls/WindowControls` draws minimise, maximise/restore and close. Anything clickable inside the
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
Payments share one `StoreDirectory` read from the server, so a new store or account appears in both. `TransactionsView` filters by store, type and status and opens a detail drawer
with the ledger entries; `AccountsView` shows the accounts with their share of the total and recent ledger entries;
`StaffView` lists the team with an "Add staff" dialog. `BeadIllustration` is the line-art used for empty states.

### Hiding amounts

The eye button in the top bar masks every money figure (`₦ ••••••`) so nobody reading over your shoulder sees them.
Show an amount with `<controls:Money Amount="{Binding Balance}" />`, never a plain TextBlock with a converter, or
it will ignore the switch. The switch is `Services/PrivacySettings`; it resets to visible each time the workspace opens. The overview uses
sample data from `DemoData` until the backend exists.

## Sidebar, user menu and sign-out

The sidebar is near-black, with white as the selected row. Collapsed, each nav icon is centred and the name moves
into a tooltip. Clicking the avatar (top bar) or the owner card (sidebar) opens the user menu: profile details,
show or hide amounts, and sign out. Sign out asks for confirmation, then returns to a fresh sign-in screen
(`WorkspaceViewModel.SignedOut` is handled by `ShellViewModel`). Dialogs are overlays in `WorkspaceView`.
`BeadIllustration` follows the inherited text colour, so set `Foreground` to use it on a dark surface.

## Clickable rows and details

Every row or card that stands for a thing opens its details in a dialog: a transaction (drawer), a staff member, a
store, an account and a ledger entry. Make a table row clickable with `Button.TableRow` and a card with
`Button.Plain`, both with a `CommandParameter="{Binding}"`; the page's view model keeps the `Selected…` item and an
`Is…Open` flag, and the dialog is an overlay in the page's view. Editing inside these dialogs comes later, once
roles are enforced.

## Releasing

```
powershell -ExecutionPolicy Bypass -File installer\build.ps1
```

This publishes the app as one self-contained `win-x64` exe in `publish/`, then builds the installer
`dist/Mercury-Setup.exe` with [Inno Setup 6](https://jrsoftware.org/isdl.php) (install it once, for example with
`winget install JRSoftware.InnoSetup`). Both folders are git-ignored.

- **Icon:** `Resources/App.ico` (the Mercury mark on a near-black tile) is the exe, window, shortcut and installer icon.
- **Version:** change `<Version>`, `<AssemblyVersion>` and `<FileVersion>` in `Mercury.Desktop.csproj`, and `AppVersion`
  in `installer/Mercury.iss`, together.
- **Installer:** per-user by default (no admin prompt), adds a Start menu entry and an optional desktop shortcut, and
  registers an uninstaller in Settings, Apps.
- The app and installer are not code-signed yet, so Windows SmartScreen shows a warning on first run.

## Talking to the server

The app reads and writes through `Services/Api`. View models depend on `IMercuryApi`, never on HTTP:

- `HttpMercuryApi` (over `ApiClient`) is the real server. The address comes from the `MERCURY_API_URL` environment variable
  and defaults to `http://localhost:5100/`.
- `DemoMercuryApi` serves built-in sample data. The developer switches (`--dashboard`, `--shots`, `--design-system`) use it,
  so every screen can be reviewed without a server.
- `ApiClient` adds the sign-in token, reads the JSON and turns every failure into an `ApiException` whose message is
  ready to show. A rejected token raises `Unauthorized`, and `ShellViewModel` returns to sign-in.
- `Dtos.cs` mirrors `Mercury.Api/DTOs`; keep the two in step.

**Signing in.** The token is stored encrypted for the current Windows user (`TokenStore`, in `%LOCALAPPDATA%\Mercury`), so the
next launch goes straight in. `ShellViewModel.InitializeAsync` checks it with `GET /auth/me`; signing out clears it.

**Onboarding** creates the business when the first-store step is completed (`POST /auth/register/merchant`, then
`POST /merchant/stores`), so the later screens only appear once it exists; going back from the payment step is disabled for
the same reason. The payment-account step then asks for the first store's account (`POST /stores/{id}/collection-accounts`):
a Mercury account (the owner's BVN, bank and account number, passed to the provider and not kept) or one the business already
uses, or "Skip for now" to do it later from Payments. Passwords are checked against the server's rule (`PasswordPolicy`)
before they are sent.

**Joining from an invitation.** Sign-in has "Join with your invitation": paste the link or code, choose a password
(`POST /auth/invitations/accept`) and the person is signed in. The installer registers `mercury://` links, so opening an
invitation link starts the app on the same screen with the code already filled in (`InvitationLink`,
`AppServices.PendingInvitationToken`). Opening a link while Mercury is already running starts a second window; there is no
single-instance hand-over yet.

**Roles.** `GET /auth/me` says the role; `Permissions` decides which pages and buttons it gets (Owner and Manager all pages;
Accountant no Payments or Staff; Cashier no Accounts or Staff; only Owner adds stores; Owner and Manager invite staff). This
hides what a person cannot use. The server enforces the same rules.

**What is live.** The overview, Stores, Payments, Transactions and Accounts are read from the server, each with a loading state
and a retry. Stores (`GET /merchant/stores`) shows each store with its balance (hidden for cashiers) and the accounts it
collects into; Payments shows the account a customer pays into for the chosen store, and lets an owner add one: a Mercury
account (BVN, bank and account number, passed to the provider and not kept; it stays "Setting up" until the bank confirms) or
an account the business already uses. Transactions (`GET /transactions`, `/transactions/{id}`) filters by store and type on
the server and by status here, and opens the ledger entries behind a row; Accounts (`GET /accounts`) shows the money-holding
accounts with their share of the total and the latest ledger entries. Staff (`GET /staff`) lists the team the person may
see, and "Add staff" sends an invitation (`POST /staff/invitations`): the invited person is not on the list until they
accept and choose their own password. An owner can invite managers, cashiers and accountants; a manager can only invite
cashiers, and only to their own store, so that choice is locked. Every page is now read from the server.
`PageViewModel.IsSampleData` and the "Sample data" label remain for any page that is ever added before it is wired.

**Invitations and email.** The server emails the invitation when an SMTP server is set up (see the backend README, which covers
Gmail); the email carries the code and a `mercury://` link. Until then the server only logs the link, so it has to be passed
to the person by hand.
