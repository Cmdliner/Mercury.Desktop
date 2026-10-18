using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Shows the account a customer pays into for a store, ready to read out or copy.</summary>
public sealed partial class PaymentsViewModel : PageViewModel
{
    public PaymentsViewModel(ObservableCollection<StoreAccount> stores, string businessName)
    {
        Stores = stores;
        AccountName = businessName;
        _selectedStore = stores.FirstOrDefault();
    }

    public override string Title => "Payments";

    public ObservableCollection<StoreAccount> Stores { get; }

    /// <summary>The name the bank shows to a customer before they confirm a transfer.</summary>
    public string AccountName { get; }

    [ObservableProperty]
    private StoreAccount? _selectedStore;

    [ObservableProperty]
    private bool _justCopied;

    [RelayCommand]
    private async Task CopyAccountNumberAsync()
    {
        if (SelectedStore is null)
        {
            return;
        }

        try
        {
            Clipboard.SetText(SelectedStore.AccountNumber);
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
}
