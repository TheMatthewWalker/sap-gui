using Microsoft.UI.Xaml;
using sap_gui.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace sap_gui
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public MainWindow()
        {
            this.InitializeComponent();

            // Set initial page in frame
            MainFrame.Navigate(typeof(LoginPage));
        }
    }
}
