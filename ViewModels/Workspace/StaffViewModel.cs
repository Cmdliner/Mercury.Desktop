using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// Everyone the person can see, read from the server, with a way to invite someone. An invited
/// person is not on the list until they accept and choose their own password.
/// </summary>
public sealed partial class StaffViewModel(IMercuryApi api, StoreDirectory directory, MeResponse me) : PageViewModel
{
    public override string Title => "Staff";

    public ObservableCollection<StaffMember> Staff { get; } = [];

    /// <summary>Owners and managers can invite; accountants and cashiers cannot.</summary>
    public bool CanInvite { get; init; } = true;

    // ---- loading ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    private bool _isLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => ErrorMessage is not null;

    public bool IsLoaded => !IsLoading && !HasError;

    [ObservableProperty]
    private bool _hasStaff;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var people = await api.GetStaffAsync();
            Staff.Clear();
            foreach (var person in people) Staff.Add(ToMember(person));
            HasStaff = Staff.Count > 0;
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ---- detail ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailOpen))]
    private StaffMember? _selected;

    public bool IsDetailOpen => Selected is not null;

    [RelayCommand]
    private void Open(StaffMember person) => Selected = person;

    [RelayCommand]
    private void CloseDetail() => Selected = null;

    // ---- inviting ----

    [ObservableProperty]
    private bool _isInviting;

    [ObservableProperty]
    private InviteStaffFormViewModel _form = new(["Cashier"], []);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string? _formError;

    public bool HasFormError => !string.IsNullOrEmpty(FormError);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendInvitationCommand))]
    [NotifyPropertyChangedFor(nameof(SendLabel))]
    private bool _isSaving;

    public string SendLabel => IsSaving ? "Sending..." : "Send invitation";

    /// <summary>The email just invited, shown in place of the form once the invitation is made.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInviteSent))]
    [NotifyPropertyChangedFor(nameof(IsFormShown))]
    private string? _invitedEmail;

    public bool IsInviteSent => InvitedEmail is not null;

    public bool IsFormShown => !IsInviteSent;

    [RelayCommand]
    private void OpenInvite()
    {
        var storeNames = directory.Stores.Select(s => s.Name).ToList();

        // A manager can only invite cashiers, and only to their own store.
        Form = me.Role == StaffRole.Manager
            ? new InviteStaffFormViewModel(["Cashier"], storeNames, directory.Stores.FirstOrDefault(s => s.Id == me.StoreId)?.Name)
            : new InviteStaffFormViewModel(["Manager", "Cashier", "Accountant"], storeNames);

        FormError = null;
        InvitedEmail = null;
        IsInviting = true;
    }

    [RelayCommand]
    private void CloseInvite() => IsInviting = false;

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendInvitationAsync()
    {
        FormError = null;

        var request = Form.TryBuild(name => directory.Stores.FirstOrDefault(s => s.Name == name)?.Id);
        if (request is null) return;

        IsSaving = true;
        try
        {
            await api.InviteStaffAsync(request);
            InvitedEmail = request.Email;
        }
        catch (ApiException ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSend() => !IsSaving;

    // ---- helpers ----

    private static StaffMember ToMember(StaffResponse p) => new(
        p.Name,
        p.Email,
        p.Role.ToString(),
        p.StoreName ?? "All stores",
        p.Role switch
        {
            StaffRole.Owner => "Full",
            StaffRole.Accountant => "Financial",
            _ => "Store",
        });
}
