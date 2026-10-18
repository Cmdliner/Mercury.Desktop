using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// Shows the account a customer pays into for a store, ready to read out or copy, and lets the
/// owner add one. A store can have several accounts; the first active one is shown as the main.
/// </summary>
public sealed partial class PaymentsViewModel : PageViewModel
{
    private readonly StoreDirectory _directory;
    private Guid? _lastSelectedId;

    public PaymentsViewModel(StoreDirectory directory, string businessName)
    {
        _directory = directory;
        AccountName = businessName;
        _selectedStore = directory.Stores.FirstOrDefault();

        // A reload clears the list and refills it with new objects, so choose again only once it has
        // finished: keep the same store selected, or fall back to the first.
        directory.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(StoreDirectory.IsLoading) && !_directory.IsLoading) Reselect();
        };
    }

    public override string Title => "Payments";

    public StoreDirectory Directory => _directory;

    public ObservableCollection<StoreInfo> Stores => _directory.Stores;

    /// <summary>Only owners add payment accounts.</summary>
    public bool CanAddAccount { get; init; } = true;

    /// <summary>The name the bank shows to a customer before they confirm a transfer.</summary>
    public string AccountName { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Primary))]
    [NotifyPropertyChangedFor(nameof(HasStore))]
    private StoreInfo? _selectedStore;

    public bool HasStore => SelectedStore is not null;

    partial void OnSelectedStoreChanged(StoreInfo? value)
    {
        if (value is not null) _lastSelectedId = value.Id;
    }

    public CollectionAccountInfo? Primary => SelectedStore?.Primary;

    [RelayCommand]
    private Task RefreshAsync() => _directory.LoadAsync();

    [ObservableProperty]
    private bool _justCopied;

    [RelayCommand]
    private async Task CopyAccountNumberAsync()
    {
        if (Primary?.AccountNumber is not { } number) return;

        try
        {
            Clipboard.SetText(number);
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            // Another app is holding the clipboard; the number is still on screen to read out.
            return;
        }

        JustCopied = true;
        await Task.Delay(1800);
        JustCopied = false;
    }

    // ---- adding an account ----

    [ObservableProperty]
    private bool _isAdding;

    /// <summary>True to ask Mercury for a virtual account; false to record an account the business already uses.</summary>
    [ObservableProperty]
    private bool _useMercuryAccount = true;

    [ObservableProperty]
    private LinkAccountFormViewModel _linkForm = new();

    [ObservableProperty]
    private MercuryAccountFormViewModel _mercuryForm = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string? _formError;

    public bool HasFormError => !string.IsNullOrEmpty(FormError);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAccountCommand))]
    [NotifyPropertyChangedFor(nameof(AddLabel))]
    private bool _isSaving;

    public string AddLabel => IsSaving ? "Adding..." : UseMercuryAccount ? "Create account" : "Add account";

    partial void OnUseMercuryAccountChanged(bool value) => OnPropertyChanged(nameof(AddLabel));

    [RelayCommand]
    private void OpenAdd()
    {
        LinkForm = new LinkAccountFormViewModel();
        MercuryForm = new MercuryAccountFormViewModel();
        FormError = null;
        IsAdding = true;
    }

    [RelayCommand]
    private void CloseAdd() => IsAdding = false;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddAccountAsync()
    {
        FormError = null;
        if (SelectedStore is null) return;

        var request = UseMercuryAccount ? MercuryForm.TryBuild() : LinkForm.TryBuild();
        if (request is null) return;

        IsSaving = true;
        try
        {
            FormError = await _directory.AddAccountAsync(SelectedStore.Id, request);
            if (FormError is null) IsAdding = false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanAdd() => !IsSaving;

    private void Reselect()
    {
        SelectedStore = _directory.Stores.FirstOrDefault(s => s.Id == _lastSelectedId) ?? _directory.Stores.FirstOrDefault();
    }
}
