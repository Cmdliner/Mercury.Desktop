using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Everyone who works in the business, with a form to add someone.</summary>
public sealed partial class StaffViewModel : PageViewModel
{
    private readonly ObservableCollection<StoreAccount> _stores;

    public StaffViewModel(IEnumerable<StaffMember> staff, ObservableCollection<StoreAccount> stores)
    {
        _stores = stores;
        Staff = new ObservableCollection<StaffMember>(staff);
        _form = NewForm();
    }

    public override string Title => "Staff";

    public ObservableCollection<StaffMember> Staff { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailOpen))]
    private StaffMember? _selected;

    public bool IsDetailOpen => Selected is not null;

    [RelayCommand]
    private void Open(StaffMember person) => Selected = person;

    [RelayCommand]
    private void CloseDetail() => Selected = null;

    [ObservableProperty]
    private bool _isInviting;

    [ObservableProperty]
    private InviteStaffFormViewModel _form;

    [RelayCommand]
    private void OpenInvite()
    {
        Form = NewForm();
        IsInviting = true;
    }

    [RelayCommand]
    private void CloseInvite() => IsInviting = false;

    [RelayCommand]
    private void AddStaff()
    {
        if (Form.TryCreate() is { } person)
        {
            // TODO: send the invitation through the backend.
            Staff.Add(person);
            IsInviting = false;
        }
    }

    private InviteStaffFormViewModel NewForm() =>
        new(["All stores", .. _stores.Select(s => s.Name)]);
}
