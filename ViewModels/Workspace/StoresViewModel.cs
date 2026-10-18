using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Every store of the business, with a form to add another.</summary>
public sealed partial class StoresViewModel(ObservableCollection<StoreAccount> stores) : PageViewModel
{
    public override string Title => "Stores";

    public ObservableCollection<StoreAccount> Stores { get; } = stores;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailOpen))]
    private StoreAccount? _selected;

    public bool IsDetailOpen => Selected is not null;

    [RelayCommand]
    private void OpenDetail(StoreAccount store) => Selected = store;

    [RelayCommand]
    private void CloseDetail() => Selected = null;

    [ObservableProperty]
    private bool _isAdding;

    [ObservableProperty]
    private AddStoreFormViewModel _form = new();

    [RelayCommand]
    private void OpenAddStore()
    {
        Form = new AddStoreFormViewModel();
        IsAdding = true;
    }

    [RelayCommand]
    private void CloseAddStore() => IsAdding = false;

    [RelayCommand]
    private void CreateStore()
    {
        if (Form.TryCreate() is { } store)
        {
            // TODO: create the store on the backend and use the account it returns.
            Stores.Add(store);
            IsAdding = false;
        }
    }
}
