using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using sap_gui.Pages;
using SAPFunctionsOCX;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace sap_gui.Pages
{

    public sealed partial class PicksheetPage : Page
    {

        public PicksheetPage()
        {
            this.InitializeComponent();
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }



    }

}

