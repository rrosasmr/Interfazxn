using Avalonia;
using Interfazxn.Views;

namespace Interfazxn
{
    public partial class App : Application
    {
        public override void OnFrameworkInitializationCompleted()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            
            base.OnFrameworkInitializationCompleted();
        }
    }
}