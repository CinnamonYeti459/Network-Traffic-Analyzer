using Avalonia.Controls;
using Avalonia.Input;
using Network_Traffic_Analyzer.ViewModels;
using System.Diagnostics;

namespace Network_Traffic_Analyzer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        ConsoleDebug.ShowConsole();

        var viewModel = new MainViewModel();
        DataContext = viewModel;
    }

    private void GitHubLink_Click(object sender, PointerPressedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/CinnamonYeti459",
            UseShellExecute = true
        });
    }
}
