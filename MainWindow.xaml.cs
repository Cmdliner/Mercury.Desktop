using System.Windows;
using Mercury.Desktop.Services;
using Mercury.Desktop.ViewModels;

namespace Mercury.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel? shellViewModel = null)
    {
        InitializeComponent();
        FitToWorkArea();
        var shell = shellViewModel ?? new ShellViewModel(AppServices.CreateApi(), new TokenStore());
        DataContext = shell;
        Loaded += async (_, _) => await shell.InitializeAsync();
        StateChanged += (_, _) => FitToScreen();
    }

    // On small or highly scaled screens the designed size would spill off the display.
    private void FitToWorkArea()
    {
        var area = SystemParameters.WorkArea;
        MinWidth = Math.Min(MinWidth, area.Width);
        MinHeight = Math.Min(MinHeight, area.Height);
        Width = Math.Min(Width, area.Width);
        Height = Math.Min(Height, area.Height);
    }

    // WindowStyle=None windows extend by the resize border when maximised, hiding the edges.
    private void FitToScreen()
    {
        RootFrame.Margin = WindowState == WindowState.Maximized
            ? SystemParameters.WindowResizeBorderThickness
            : new Thickness(0);
    }
}
