using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAPFunctionsOCX;
using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using sap_gui.Pages;
using CommunityToolkit.Common;
using CommunityToolkit.WinUI; // for AdvancedCollectionView


namespace sap_gui.Pages
{
    public class TableLQUA
    {
        public string LGORT {  get; set; }
        public string MATNR { get; set; }
        public string CHARG { get; set; }
        public decimal GESME { get; set; }
        public string LGTYP { get; set; }
        public string LGPLA { get; set; }
        public string BESTQ { get; set; }
        public string SOBKZ { get; set; }
        public string SONUM { get; set; }
    }

    public sealed partial class StockPage : Page
    {

        public StockPage()
        {
            this.InitializeComponent();
        }

        private void Read_Table_Click(object sender, RoutedEventArgs e)
        {

            StatusText.Text = "Reading SAP table...";
            SapController sap = new SapController();

            try
            {
                string[] fields = { "LGORT","MATNR", "CHARG", "GESME", "LGTYP", "LGPLA", "BESTQ", "SOBKZ", "SONUM" };
                string[] options = { "LGNUM eq '312'", "", "", "", "" };

                if (!string.IsNullOrEmpty(MATNR.Text))
                {
                    string matnr = sap.SapN2C(MATNR.Text, 18);
                    options[1] = $" AND MATNR eq '{matnr}'";
                }
                if (!string.IsNullOrEmpty(CHARG.Text))
                {
                    string charg = sap.SapN2C(CHARG.Text, 10);
                    options[2] = $" AND CHARG eq '{charg}'";
                }
                if (!string.IsNullOrEmpty(LGTYP.Text))
                {
                    string lgtyp = sap.SapN2C(LGTYP.Text, 3);
                    options[3] = $" AND LGTYP eq '{lgtyp}'";
                }
                if (!string.IsNullOrEmpty(LGPLA.Text))
                {
                    options[4] = $" AND LGPLA eq '{sap.SapN2C(LGPLA.Text, 10)}'";
                }


                var rows = sap.ReadSapTable("LQUA",fields, options);
                var sortedRows = rows.OrderBy(r => r.LGPLA).ToList();

                SapDataGrid.ItemsSource = sortedRows;
                SapDataGrid.CanUserSortColumns = true;
                StatusText.Text = $"Loaded {rows.Count} rows.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private object _contextRow;
        private void SapDataGrid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            // Determine which row was clicked
            var row = (e.OriginalSource as FrameworkElement)?.DataContext;

            if (row != null)
                _contextRow = row;   // store the row for the context menu actions
        }

        private void CreateMovement_Click(object sender, RoutedEventArgs e)
        {
            if (_contextRow is TableLQUA row)
            {
                // Navigate to your warehouse movement page
                Frame.Navigate(typeof(TransferPage), row);
            }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (_contextRow is TableLQUA row)
            {
                StatusText.Text = $"Viewing history for {row.MATNR} - {row.CHARG}";
                //Frame.Navigate(typeof(MaterialHistoryPage), row);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

