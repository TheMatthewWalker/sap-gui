using Microsoft.UI.Xaml;
using SAPFunctionsOCX;
using sap_gui.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace sap_gui
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    /// 


    public partial class App : Application
    {
        private Window? _window;

        public static SAPFunctions SapFuncs { get; internal set; }
        public static string CurrentUser { get; set; } = string.Empty;
        public static bool IsLoggedIn { get; set; } = false;
        public static string CurrentPass { get; set; }  = string.Empty;


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            //this.UnhandledException += App_UnhandledException;

            // Initialize COM object once, on main STA thread
            SapFuncs = new SAPFunctions();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Debug.WriteLine(e.Exception);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
