using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Liman.WinUiExample
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    [LimanService(LimanServiceLifetime.Application)]
    public sealed partial class MainWindow : Window, ILimanRunnable
    {
        public MainWindow(IMyService myService)
        {
            this.InitializeComponent();
        }

        public void Run()
        {
            this.Activate();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }
    }
}
