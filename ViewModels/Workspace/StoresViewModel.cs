using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Every store the person can see, with a form to add another.</summary>
public sealed partial class StoresViewModel(StoreDirectory directory) : PageViewModel
{
    public override string Title => "Stores";

    public StoreDirectory Directory { get; } = directory;

    public ObservableCollection<StoreInfo> Stores => Directory.Stores;

    /// <summary>Only owners create stores.</summary>
    public bool CanAddStore { get; init; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailOpen))]
    private StoreInfo? _selected;

    public bool IsDetailOpen => Selected is not null;

    [RelayCommand]
    private void OpenDetail(StoreInfo store) => Selected = store;

    [RelayCommand]
    private void CloseDetail() => Selected = null;

    [RelayCommand]
    private Task ReloadAsync() => Directory.LoadAsync();

    [ObservableProperty]
    private bool _isAdding;

    [ObservableProperty]
    private AddStoreFormViewModel _form = new();

    /// <summary>Why the store could not be created, shown in the dialog.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string? _formError;

    public bool HasFormError => !string.IsNullOrEmpty(FormError);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateStoreCommand))]
    [NotifyPropertyChangedFor(nameof(CreateLabel))]
    private bool _isSaving;

    public string CreateLabel => IsSaving ? "Creating..." : "Create store";

    [RelayCommand]
    private void OpenAddStore()
    {
        Form = new AddStoreFormViewModel();
        FormError = null;
        IsAdding = true;
    }

    [RelayCommand]
    private void CloseAddStore() => IsAdding = false;

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateStoreAsync()
    {
        FormError = null;
        if (Form.TryGet() is not var (name, location)) return;

        IsSaving = true;
        try
        {
            FormError = await Directory.AddStoreAsync(name, location);
            if (FormError is null) IsAdding = false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanCreate() => !IsSaving;
}
