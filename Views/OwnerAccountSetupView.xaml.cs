using System.Windows.Controls;

namespace Mercury.Desktop.Views;

public partial class OwnerAccountSetupView : UserControl
{
    public OwnerAccountSetupView()
    {
        InitializeComponent();
        DataContext = new OwnerAccountSetupView();
    }
}