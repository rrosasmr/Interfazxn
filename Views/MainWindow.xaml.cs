using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Interfazxn.ViewModels;

namespace Interfazxn.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = new MainViewModel();
        }
    }
}