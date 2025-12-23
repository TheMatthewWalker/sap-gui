using Avalonia.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using sap_gui.Pages;
using SAPFunctionsOCX;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Windows.Gaming.XboxLive;

namespace sap_gui.Pages
{

    public sealed partial class TransferPage : Page
    {

        public TransferPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is TableLQUA row)
            {
                DataContext = row;  // auto-populates textboxes
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }


        private void CreateTransfer_Click(object sender, RoutedEventArgs e)
        {
            var sap = new SapController();
            float VERME;

            // Call your controller method
            var result = sap.CreateTransferOrder(
                plant: "3012",
                sloc: LGORT.Text,
                warehouse: "312",
                binType: LGTYP.Text,
                bin: sap.SapN2C(LGPLA.Text, 10),
                material: sap.SapN2C(MATNR.Text, 18),
                batch: sap.SapN2C(CHARG.Text, 10),
                qty: float.TryParse(GESME.Text, out VERME) ? VERME : 0,
                destBin: sap.SapN2C(NLPLA.Text, 10),
                destBinType: NLTYP.Text,
                category: BESTQ.Text,
                special: SOBKZ.Text,
                specialNumber: sap.SapN2C(SONUM.Text, 16)
            );

            if (result.Success)
            {
                SapResultBox.Text = $"Stock has been transfer with TR Number: {result.TransferOrderNumber}";
            }
            else
            {
                SapResultBox.Text = result.Error;
            }
        }



    }

}

