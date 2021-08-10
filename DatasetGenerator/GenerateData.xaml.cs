using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class GenerateData : Page
    {
        public GenerateData()
        {
            InitializeComponent();
        }

        private void GridView_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in Transmitter.Datasets)
            {
                Gv_Datasets.Items.Add(item);
            }
        }

        private void Gv_Datasets_ItemClick(object sender, ItemClickEventArgs e)
        {
            Transmitter.CurrentlyEditedDataset = (Dataset)e.ClickedItem;
            Frame.Navigate(typeof(NewDataEditor));
        }
    }
}
