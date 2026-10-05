using System.Windows.Controls;
using Mercury.Desktop.ViewModels;

namespace Mercury.Desktop.Views;

public partial class MerchantSetupView : UserControl
{
    public MerchantSetupView()
    {
        InitializeComponent();
        DataContext = new MerchantSetupViewModel();
    }
}