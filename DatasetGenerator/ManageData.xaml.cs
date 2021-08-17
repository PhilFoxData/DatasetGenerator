using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;
using System.Threading.Tasks;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ManageData : Page
    {
        private readonly uint NUMBER_OF_ITEMS_TO_LOAD = 100;


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

            if (folder == null)
            {
                return;
            }

            Txb_Progress.Text = "Exportiere...";
            Stc_ExportProgress.Visibility = Visibility.Visible;

            for (int i = 0; i < 10; i++)
            {
                StorageFolder targetFolder = await folder.CreateFolderAsync(i.ToString(),
                        CreationCollisionOption.ReplaceExisting);
                StorageFolder sourceFolder = await currentDSFolder.GetFolderAsync(i.ToString());

                foreach (StorageFile file in await sourceFolder.GetFilesAsync())
                {
                    await file.CopyAsync(targetFolder, file.Name, NameCollisionOption.ReplaceExisting);
                }
            }


            Stc_ExportProgress.Visibility = Visibility.Collapsed;
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
                Gv_Data.Items.Clear();

                Cmd_Export.IsEnabled = true;
                Cmd_DeleteDataset.IsEnabled = true;

                Load_Images((Lsv_Datasets.SelectedItem as Dataset).NamesOfFiles,
                        (Lsv_Datasets.SelectedItem as Dataset).Name);

            }
        }

        private async void Load_Images(List<string>[] fileNames, string datasetName)
        {
            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            StorageFolder currentDatasetFolder = await datasetsFolder.GetFolderAsync(datasetName);

            int numberOfLoadedImages = 0;

            for (int i = 0; i < 10; i++)
            {
                StorageFolder currentLabelsFolder = await currentDatasetFolder.GetFolderAsync(i.ToString());

                foreach (var item in fileNames[i])
                {
                    DataItem dataItem = new DataItem
                    {
                        Label = i.ToString(),
                        ImageSourceFileName = item
                    };

                    StorageFile imageFile = await currentLabelsFolder.GetFileAsync(item);

                    using (var stream = await imageFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        dataItem.Image = bitmap;
                    }

                    Gv_Data.Items.Add(dataItem);

                    numberOfLoadedImages++;

                    if (numberOfLoadedImages >= NUMBER_OF_ITEMS_TO_LOAD)
                    {
                        return;
                    }
                }
            }
        }

        private async void Cmd_DeleteDataset_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ConfirmDeleteDialog();

            Transmitter.DeleteDatsetName = (Lsv_Datasets.SelectedItem as Dataset).Name;

            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");

                string nameOfTargetFolder = (Lsv_Datasets.SelectedItem as Dataset).Name;

                StorageFolder targetDataset = await datasetsFolder.GetFolderAsync(nameOfTargetFolder);

                await targetDataset.DeleteAsync(StorageDeleteOption.PermanentDelete);

                Cmd_DeleteDataset.IsEnabled = false;

                Transmitter.Datasets.Remove(Lsv_Datasets.SelectedItem as  Dataset);
                Lsv_Datasets.Items.Remove(Lsv_Datasets.SelectedItem);
                Gv_Data.ItemsSource = null;
                Gv_Data.Items.Clear();
            }
        }

        private async void Cmd_Import_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            StorageFolder sourceFolder = await picker.PickSingleFolderAsync();

            if (sourceFolder == null)
            {
                return;
            }

            ContentDialog dialog = new NewDatasetDialog();

            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                Txb_Progress.Text = "Importiere...";
                Stc_ExportProgress.Visibility = Visibility.Visible;

                Dataset newDataset = Transmitter.NewDataset;
                await newDataset.CreateDataset();

                StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
                StorageFolder targetFolder = await datasetsFolder.GetFolderAsync(Transmitter.NewDataset.Name);

                StorageFile jsonFile = await targetFolder.GetFileAsync("Dataset_Info.json");

                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        StorageFolder currentLabelsSourceFolder = await sourceFolder.GetFolderAsync(i.ToString());
                        StorageFolder currentLabelsTargetFolder = await targetFolder.GetFolderAsync(i.ToString());

                        foreach (var item in await currentLabelsSourceFolder.GetFilesAsync())
                        {
                            string newName = (await item.CopyAsync(currentLabelsTargetFolder, "Image.png",
                                    NameCollisionOption.GenerateUniqueName)).Name;
                            newDataset.NamesOfFiles[i].Add(newName);
                        }
                    } 

                    await FileIO.WriteTextAsync(jsonFile, JsonSerializer.Serialize(newDataset));

                    Transmitter.Datasets.Add(newDataset);
                    Lsv_Datasets.Items.Add(newDataset);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            Stc_ExportProgress.Visibility = Visibility.Collapsed;
        }

        private async Task Import_First_Generation()
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            StorageFolder sourceFolder = await picker.PickSingleFolderAsync();

            if (sourceFolder == null)
            {
                return;
            }

            ContentDialog dialog = new NewDatasetDialog();

            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                Stc_ExportProgress.Visibility = Visibility.Visible;

                Dataset newDataset = Transmitter.NewDataset;
                await newDataset.CreateDataset();

                try
                {
                    string allLabelsString = "";

                    StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
                    StorageFolder targetFolder = await datasetsFolder.GetFolderAsync(newDataset.Name);

                    StorageFile initalLabelsFile = await sourceFolder.TryGetItemAsync("Labels.txt") as StorageFile;

                    if (initalLabelsFile != null)
                    {
                        allLabelsString += await FileIO.ReadTextAsync(initalLabelsFile);
                    }

                    int nextLabelsFileIterator = 2;
                    StorageFile nextLabelsFile = await sourceFolder.TryGetItemAsync($"Labels (2).txt") as StorageFile;

                    while (nextLabelsFile != null)
                    {
                        allLabelsString += await FileIO.ReadTextAsync(nextLabelsFile);
                        nextLabelsFileIterator++;
                        nextLabelsFile = await sourceFolder.TryGetItemAsync($"Labels ({nextLabelsFileIterator}).png") as StorageFile;
                    }

                    StorageFile initialImageFile = await sourceFolder.TryGetItemAsync("Image_1.png") as StorageFile;
                    if (initialImageFile != null)
                    {
                        string initalImageFileLabel = allLabelsString[0].ToString();

                        StorageFolder initialImageLabelFolder = await targetFolder.GetFolderAsync(initalImageFileLabel);
                        await initialImageFile.CopyAsync(targetFolder, "Image.png");
                        newDataset.NamesOfFiles[Convert.ToInt32(initalImageFileLabel)].Add("Image.png");
                    }

                    int nextImageFileIterator = 2;
                    StorageFile nextImageFile = await sourceFolder.TryGetItemAsync(
                            $"Image_{nextImageFileIterator}.png") as StorageFile;

                    while (nextImageFile != null)
                    {
                        int currentLabel = Convert.ToInt32(allLabelsString[nextImageFileIterator * 2 - 2].ToString());

                        StorageFolder targetLabelsFolder = await targetFolder.GetFolderAsync(currentLabel.ToString());

                        StorageFile copiedFile = await nextImageFile.CopyAsync(targetLabelsFolder,
                                "Image.png", NameCollisionOption.GenerateUniqueName);

                        newDataset.NamesOfFiles[currentLabel].Add(copiedFile.Name);

                        nextImageFileIterator++;
                        nextImageFile = await sourceFolder.TryGetItemAsync($"Image_{nextImageFileIterator}.png")
                                as StorageFile;
                    }

                    StorageFile jsonFile = await targetFolder.GetFileAsync("Dataset_Info.json");
                    await FileIO.WriteTextAsync(jsonFile, JsonSerializer.Serialize(newDataset));
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                Stc_ExportProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void Gv_Data_ItemClick(object sender, ItemClickEventArgs e)
        {
            DataItem currentItem = (DataItem)e.ClickedItem;
            Dataset currentDataset = Lsv_Datasets.SelectedItem as Dataset;

            ContentDialog dialog = new ContentDialog
            {
                Title = "Diesen Eintrag löschen",
                PrimaryButtonText = "Abbrechen",
                SecondaryButtonText = "OK",
            };

            StackPanel stc_DialogContent = new StackPanel();
            stc_DialogContent.Orientation = Orientation.Vertical;

            TextBlock txb_Message = new TextBlock
            {
                Text = "Dieser Eintrag wird unwideruflich gelöscht."
            };

            Image img_Image = new Image
            {
                Source = currentItem.Image,
                Margin = new Thickness(0, 10, 0, 0),
                Height = 100,
                Width = 100
            };

            stc_DialogContent.Children.Add(txb_Message);
            stc_DialogContent.Children.Add(img_Image);

            dialog.Content = stc_DialogContent;

            if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
                StorageFolder currentDatasetFolder = await datasetsFolder.GetFolderAsync(currentDataset.Name);
                StorageFolder currentLabelFolder = await currentDatasetFolder.GetFolderAsync(currentItem.Label);

                StorageFile jsonFile = await currentDatasetFolder.GetFileAsync("Dataset_Info.json");
                StorageFile targetFile = await currentLabelFolder.GetFileAsync(currentItem.ImageSourceFileName);

                currentDataset.NamesOfFiles[Convert.ToInt32(currentItem.Label)].Remove(currentItem.ImageSourceFileName);
                
                await FileIO.WriteTextAsync(jsonFile, JsonSerializer.Serialize(currentDataset));
                await targetFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                Gv_Data.Items.Remove(e.ClickedItem);
            }
        }
    }
}
