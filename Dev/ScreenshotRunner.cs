#if DEBUG
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Mercury.Desktop.Models;
using Mercury.Desktop.ViewModels;
using Mercury.Desktop.ViewModels.Onboarding;
using Mercury.Desktop.Views.DesignSystem;

namespace Mercury.Desktop.Dev;

/// <summary>
/// Walks through onboarding the way a user would (empty, with errors, valid) and saves a PNG
/// of each state, so changes to the design system can be reviewed without clicking around.
/// </summary>
internal static class ScreenshotRunner
{
    public static async Task RunAsync(string folder, Application app)
    {
        Directory.CreateDirectory(folder);

        // Closing a window must not end the app between captures.
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            await CaptureOnboardingAsync(folder, app.Dispatcher);
            await CaptureGalleryAsync(folder, app.Dispatcher);
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync(Path.Combine(folder, "error.txt"), ex.ToString());
        }
        finally
        {
            app.Shutdown();
        }
    }

    private static async Task CaptureOnboardingAsync(string folder, Dispatcher dispatcher)
    {
        var window = new MainWindow();
        var shell = (ShellViewModel)window.DataContext;
        var vm = shell.Onboarding;
        window.Show();
        await SettleAsync(dispatcher);

        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "login.png"));
        shell.Login.SignInCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "login-errors.png"));
        shell.Login.CreateAccountCommand.Execute(null);
        await SettleAsync(dispatcher);

        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-1-business.png"));

        // Continue with nothing filled in: every field reports its error.
        vm.ContinueCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-1b-business-errors.png"));

        var business = (BusinessDetailsStepViewModel)vm.CurrentStep;
        business.BusinessName = "Adeyemi Stores";
        business.BusinessEmail = "owner@adeyemistores.ng";
        business.BusinessPhone = "+234 800 000 0000";
        business.BusinessAddress = "Lagos, Nigeria";
        vm.ContinueCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-2-owner.png"));

        var owner = (OwnerAccountStepViewModel)vm.CurrentStep;
        owner.FullName = "Abdulazeez Adeyemi";
        owner.EmailAddress = "owner@adeyemistores";
        owner.Password = "mercury-2026";
        owner.ConfirmPassword = "mercury-20";
        vm.ContinueCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-2b-owner-errors.png"));

        owner.EmailAddress = "owner@adeyemistores.ng";
        owner.ConfirmPassword = "mercury-2026";
        vm.ContinueCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-3-store.png"));

        var store = (FirstStoreStepViewModel)vm.CurrentStep;
        store.StoreName = "Adeyemi Stores Ikeja";
        store.Location = "Ikeja, Lagos";
        store.VirtualAccountNumber = "01234";
        vm.ContinueCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-3b-store-errors.png"));

        store.Bank = "GTBank";
        store.VirtualAccountNumber = "0123456789";
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-3c-store-valid.png"));

        vm.ContinueCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "onboarding-4-finish.png"));

        // Finish onboarding: the shell swaps to the dashboard.
        shell.ShowWorkspace(DemoData.SampleOnboardingResult);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "dashboard-overview.png"));

        var workspace = (ViewModels.Workspace.WorkspaceViewModel)shell.Current;
        workspace.SelectedNavItem = workspace.NavItems.First(item => item.Title == "Stores");
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "stores.png"));
        var stores = (ViewModels.Workspace.StoresViewModel)workspace.CurrentPage;
        stores.OpenAddStoreCommand.Execute(null);
        await SettleAsync(dispatcher);
        stores.CreateStoreCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "stores-add.png"));
        stores.CloseAddStoreCommand.Execute(null);
        workspace.SelectedNavItem = workspace.NavItems.First(item => item.Title == "Payments");
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "payments.png"));
        workspace.SelectedNavItem = workspace.NavItems.First(item => item.Title == "Transactions");
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "transactions.png"));
        var transactions = (ViewModels.Workspace.TransactionsViewModel)workspace.CurrentPage;
        transactions.OpenCommand.Execute(transactions.View.Cast<Transaction>().First());
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "transactions-detail.png"));
        transactions.CloseDetailCommand.Execute(null);
        transactions.SelectedStatus = "Failed";
        transactions.SelectedStore = "Ikeja";
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "transactions-empty.png"));
        transactions.ClearFiltersCommand.Execute(null);

        workspace.SelectedNavItem = workspace.NavItems.First(item => item.Title == "Accounts");
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "accounts.png"));

        workspace.SelectedNavItem = workspace.NavItems.First(item => item.Title == "Staff");
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "staff.png"));
        var staff = (ViewModels.Workspace.StaffViewModel)workspace.CurrentPage;
        staff.OpenInviteCommand.Execute(null);
        staff.AddStaffCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "staff-add.png"));
        staff.CloseInviteCommand.Execute(null);

        workspace.SelectedNavItem = workspace.NavItems[0];
        workspace.ToggleAmountsCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "dashboard-hidden.png"));
        workspace.ToggleAmountsCommand.Execute(null);

        workspace.ShowProfileCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "profile.png"));
        workspace.CloseProfileCommand.Execute(null);
        workspace.RequestSignOutCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "sign-out.png"));
        workspace.CancelSignOutCommand.Execute(null);
        workspace.ToggleSidebarCommand.Execute(null);
        await SettleAsync(dispatcher);
        Save(window, (FrameworkElement)window.Content, Path.Combine(folder, "dashboard-collapsed.png"));

        window.Close();
    }

    private static async Task CaptureGalleryAsync(string folder, Dispatcher dispatcher)
    {
        var window = new DesignSystemWindow();
        window.Show();
        await SettleAsync(dispatcher);
        Save(window, window.Capturable, Path.Combine(folder, "design-system.png"));
        window.Close();
    }

    /// <summary>Waits for layout, bindings and the entrance animations to finish.</summary>
    private static async Task SettleAsync(Dispatcher dispatcher)
    {
        await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        await Task.Delay(700);
        await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static void Save(Window window, FrameworkElement element, string path)
    {
        var width = (int)Math.Ceiling(element.ActualWidth);
        var height = (int)Math.Ceiling(element.ActualHeight);

        // Paint the window background first: it is not part of the content being captured.
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var bounds = new Rect(0, 0, width, height);
            context.DrawRectangle(window.Background, null, bounds);
            // Map the element's exact layout box, not its painted bounds, or the image stretches.
            var brush = new VisualBrush(element)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(VisualTreeHelper.GetOffset(element).X, VisualTreeHelper.GetOffset(element).Y, width, height),
                Stretch = Stretch.None,
            };
            context.DrawRectangle(brush, null, bounds);
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
#endif
