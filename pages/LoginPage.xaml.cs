using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using sap_gui.Pages;
using SAPFunctionsOCX;
using System;
using System.Threading.Tasks;

namespace sap_gui.Pages
{
    public sealed partial class LoginPage : Page
    {

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

            string user = sapUsername.Text;
            string pass = sapPassword.Password;

            dynamic sapFuncs = App.SapFuncs;

            int attempts = 0;
            const int maxAttempts = 10;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;
                    dynamic conn = sapFuncs.Connection;
                    Task.Delay(250).Wait();
                    conn.System = "KAP";
                    conn.Client = "100";
                    conn.SystemID = "01";
                    conn.User = user;
                    conn.Password = pass;

                    bool connected = conn.Logon(0, true);

                    if (connected)
                    {
                        loginText.Text = "Login sucessful!";
                        //App.SapSession = sapFuncs;
                        App.CurrentUser = user;
                        App.CurrentPass = pass;
                        App.IsLoggedIn = true;
                        if (this.Parent is Frame parentFrame)
                        {
                            parentFrame.Navigate(typeof(NavPage)); // navigate to MainPage
                        }
                        break;
                    }
                }

                catch (System.Runtime.InteropServices.COMException comEx)
                {
                    // This is the error you reported
                    if (comEx.Message == "Invalid callee. (0x80020010 (DISP_E_BADCALLEE))")
                    {
                        System.Diagnostics.Debug.WriteLine($"COMException DISP_E_BADCALLEE on attempt {attempts}: {comEx.Message}");

                        //SapController sap = new SapController();
                        //sap.RecreateSapFunctions;

                        // retry AFTER a short delay
                        Task.Delay(250).Wait();
                        continue;
                    }

                    // Any other COM exception → don't retry
                    throw;
                }
                catch (Exception ex)
                {
                    // Other errors shouldn't be retried
                    throw new Exception($"SAP login failed unexpectedly: {ex.Message}", ex);
                }

                // small delay before retry for non-exception failures
                Task.Delay(250).Wait();
            }
        }
    }
}
