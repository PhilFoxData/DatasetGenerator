using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    public sealed partial class NewDatasetDialog : ContentDialog
    {
        public NewDatasetDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Transmitter.NewDataset = null;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Dataset newDataset = new Dataset
            {
                Name = Txt_Name.Text.Replace(' ', '_'),
                Discription = Txt_Discription.Text,
                DesiredLength = Convert.ToInt32(Txt_Length.Text),
                ImageResolution = Convert.ToInt32(Txt_Resolution.Text)
            };

            Transmitter.NewDataset = newDataset;
        }

        private void Txt_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            string AdjustedName = Txt_Name.Text.Replace(' ', '_');

            foreach(Dataset item in Transmitter.Datasets)
            {
                if (AdjustedName == item.Name)
                {
                    IsSecondaryButtonEnabled = false;
                }
                else
                {
                    IsSecondaryButtonEnabled = true;
                }
            }
        }
    }
}
