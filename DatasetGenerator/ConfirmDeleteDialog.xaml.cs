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

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    public sealed partial class ConfirmDeleteDialog : ContentDialog
    {
        public ConfirmDeleteDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Txb_Content.Text = $"Sie sind im Begriff den Datensatz {Transmitter.DeleteDatsetName} \n" +
                $"zu löschen. Dies kann NICHT rückgängig gemacht werden.\nMöchten sie wirklich fortfahren?";
        }
    }
}
