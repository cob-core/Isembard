using Avalonia.Controls;
using Avalonia.Markup.Xaml;
namespace Isembard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new LogInterpreter();
    }

    private void InitializeComponent()
{
    AvaloniaXamlLoader.Load(this);
}
}