using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.Services;

/// <summary>
/// The stores the signed-in person can see, loaded from the server. The Stores and Payments pages
/// share one, so adding a store or an account on either shows up on both.
/// </summary>
public sealed partial class StoreDirectory(IMercuryApi api) : ObservableObject
{
    public ObservableCollection<StoreInfo> Stores { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    [NotifyPropertyChangedFor(nameof(HasNoStores))]
    private bool _isLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasNoStores))]
    private string? _errorMessage;

    public bool HasError => ErrorMessage is not null;

    /// <summary>True once the list has been read successfully, whether or not there are stores in it.</summary>
    public bool IsLoaded => !IsLoading && !HasError;

    public bool HasStores => Stores.Count > 0;

    public bool HasNoStores => IsLoaded && Stores.Count == 0;

    /// <summary>Reads the stores again. Safe to call repeatedly, for a retry or to refresh a pending account.</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await api.GetStoresAsync();
            Stores.Clear();
            foreach (var store in response.Stores) Stores.Add(ToInfo(store));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasStores));
            OnPropertyChanged(nameof(HasNoStores));
        }
    }

    /// <returns>A message to show if the store could not be created, or null on success.</returns>
    public async Task<string?> AddStoreAsync(string name, string location)
    {
        try
        {
            await api.CreateStoreAsync(name, location);
        }
        catch (ApiException ex)
        {
            return ex.Message;
        }

        await LoadAsync();
        return null;
    }

    /// <returns>A message to show if the account could not be added, or null on success.</returns>
    public async Task<string?> AddAccountAsync(Guid storeId, CreateCollectionAccountRequest request)
    {
        try
        {
            await api.CreateCollectionAccountAsync(storeId, request);
        }
        catch (ApiException ex)
        {
            return ex.Message;
        }

        await LoadAsync();
        return null;
    }

    private static StoreInfo ToInfo(StoreResponse store) => new(
        store.Id,
        store.Name,
        store.Location,
        store.Balance,
        store.CollectionAccounts
            .Select(a => new CollectionAccountInfo(a.Id, a.Kind, a.Status, a.BankName, a.AccountNumber, a.AccountName, a.FailureReason))
            .ToList());
}
