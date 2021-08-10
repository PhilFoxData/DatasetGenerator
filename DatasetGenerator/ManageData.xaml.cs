using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.Threading;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ManageData : Page
    {
        public ManageData()
        {
            InitializeComponent();
        }

        private async void Cmd_NewDataset_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new NewDatasetDialog();
            await dialog.ShowAsync();

            if (Transmitter.NewDataset != null)
            {
                await Transmitter.NewDataset.CreateDataset();

                Transmitter.Datasets.Add(Transmitter.NewDataset);
                Lsv_Datasets.Items.Add(Transmitter.NewDataset);
            }
        }

        private async void Cmd_Export_Click(object sender, RoutedEventArgs e)
        {
            Dataset selectedDataset = (Dataset)Lsv_Datasets.SelectedItem;

            StorageFolder datasets_Folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            StorageFolder currentDSFolder = await datasets_Folder.GetFolderAsync(selectedDataset.Name);

            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();

            List<StorageFile> labelFiles = new List<StorageFile>();
            List<StorageFile> imageFiles = new List<StorageFile>();

            foreach(var file in await currentDSFolder.GetFilesAsync())
            {
                if (file.Name.Contains("Label"))
                {
                    labelFiles.Add(file);
                }
                else if (file.Name.Contains("Image"))
                {
                    imageFiles.Add(file);
                }
            }

            StorageFile finalLabelsFile = await folder.CreateFileAsync("Labels.txt", CreationCollisionOption.ReplaceExisting);

            if (await currentDSFolder.TryGetItemAsync("Labels.txt") != null)
            {
                string LabelsContent = await FileIO.ReadTextAsync(await currentDSFolder.GetFileAsync("Labels.txt"));

                await FileIO.WriteTextAsync(finalLabelsFile, LabelsContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }

            for (int i = 2; i <= labelFiles.Count; i++)
            {
                string nextLabelSet = await FileIO.ReadTextAsync(await currentDSFolder.GetFileAsync("Labels (" + i + ").txt"));

                await FileIO.AppendTextAsync(finalLabelsFile, nextLabelSet, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }

            if (await currentDSFolder.TryGetItemAsync("Image.png") != null)
            {
                StorageFile firstImageFile = await currentDSFolder.GetFileAsync("Image.png");

                await firstImageFile.CopyAsync(folder, "Image_1.png", NameCollisionOption.ReplaceExisting);
            }

            for (int i = 2; i <= imageFiles.Count; i++)
            {
                StorageFile sourceFile = await currentDSFolder.GetFileAsync("Image (" + i + ").png");

                await sourceFile.CopyAsync(folder, "Image_" + i + ".png", NameCollisionOption.ReplaceExisting);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in Transmitter.Datasets)
            {
                Lsv_Datasets.Items.Add(item);
            }
        }

        private void Lsv_Datasets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Lsv_Datasets.SelectedItem != null)
            {
                Cmd_Export.IsEnabled = true;
            }
        }
    }
}
