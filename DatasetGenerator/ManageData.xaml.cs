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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ManageData : Page
    {
        uint NUMBER_OF_ITEMS_TO_LOAD = 100;


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
            Txb_Progress.Text = "Exportiere...";
            Stc_ExportProgress.Visibility = Visibility.Visible;

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
                Cmd_Export.IsEnabled = true;
                Cmd_DeleteDataset.IsEnabled = true;

                Load_Images((Lsv_Datasets.SelectedItem as Dataset).Name);
                
            }
        }

        private async void Load_Images(string DatasetName)
        {
            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            StorageFolder sourceFolder = await datasetsFolder.GetFolderAsync(DatasetName);

            if (sourceFolder == null) { return; }

            string allLabels = "";

            StorageFile initialLabelFile = null;
            StorageFile initialImageFile = null;

            List<DataItem> dataItems = new List<DataItem>();


            initialLabelFile = await sourceFolder.TryGetItemAsync("Labels.txt") as StorageFile;
            initialImageFile = await sourceFolder.TryGetItemAsync("Image.png") as StorageFile;


            if (initialLabelFile != null && initialImageFile != null)
            {
                allLabels += await FileIO.ReadTextAsync(initialLabelFile);
                DataItem firstDataItem = new DataItem
                {
                    Label = allLabels[0].ToString()
                };

                using (var stream = await initialImageFile.OpenReadAsync())
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    firstDataItem.Image = bitmapImage;
                }

                firstDataItem.ImageSourceFileName = "Image.png";
                dataItems.Add(firstDataItem);
            }

            int labelsIndex = 2;
            while (allLabels.Length < NUMBER_OF_ITEMS_TO_LOAD * 2)
            {
                StorageFile nextfile = await sourceFolder.TryGetItemAsync($"Labels ({labelsIndex}).txt") as StorageFile;
                if (nextfile == null)
                {
                    break;
                }
                allLabels += await FileIO.ReadTextAsync(nextfile);
                labelsIndex++;
            }


            for (int i = 2; i <= allLabels.Length / 2 - 2; i++)
            {
                DataItem currentItem = new DataItem
                {
                    Label = allLabels[i * 2 - 2].ToString()
                };

                StorageFile currentSourceFile = await sourceFolder.GetFileAsync($"Image ({i}).png");
                currentItem.ImageSourceFileName = currentSourceFile.Name;

                using (var stream = await currentSourceFile.OpenReadAsync())
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    currentItem.Image = bitmapImage;
                }

                dataItems.Add(currentItem);
            }

            Gv_Data.ItemsSource = dataItems;
            
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

            Txb_Progress.Text = "Importiere...";
            Stc_ExportProgress.Visibility = Visibility.Visible;

            try
            {
                StorageFile labelsFile = await sourceFolder.GetFileAsync("Labels.txt");

                ContentDialog newDatasetDialog = new NewDatasetDialog();

                if (await newDatasetDialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    return;
                }

                await Transmitter.NewDataset.CreateDataset();

                StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
                StorageFolder targetFolder = await datasetsFolder.GetFolderAsync(Transmitter.NewDataset.Name);

                await labelsFile.CopyAsync(targetFolder);

                foreach (var item in await sourceFolder.GetFilesAsync())
                {
                    if (item.Name.Contains("Image"))
                    {
                        if (item.Name == "Image_1.png")
                        {
                            StorageFile targetFile = await item.CopyAsync(targetFolder);
                            await targetFile.RenameAsync("Image.png");
                        }
                        else
                        {
                            StorageFile targetFile = await item.CopyAsync(targetFolder);
                            string newContent = "";

                            int index = 6;

                            while (true)
                            {
                                if (item.Name[index] != '.')
                                {
                                    newContent += item.Name[index];
                                }
                                else
                                {
                                    break;
                                }

                                index++;
                            }

                            string newName = $"Image ({newContent}).png";


                            await targetFile.RenameAsync(newName);
                        }
                    }
                }

                Transmitter.Datasets.Add(Transmitter.NewDataset);
                Lsv_Datasets.Items.Add(Transmitter.NewDataset);
            }
            catch (Exception)
            {
                StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
                IStorageItem targetFolder = await datasetsFolder.TryGetItemAsync(Transmitter.NewDataset.Name);

                if (targetFolder != null)
                {
                    await targetFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Importfehler",
                    Content = "Die Daten haben das falsche Format",
                    PrimaryButtonText = "OK"
                };

                await dialog.ShowAsync();
            }

            Stc_ExportProgress.Visibility = Visibility.Collapsed;
        }

        private async void Gv_Data_ItemClick(object sender, ItemClickEventArgs e)
        {
            DataItem currentItem = (DataItem)e.ClickedItem;

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
                Text = "Dieser Eintrag wird unwideruflich gelöscht.\n" +
                        "Abhängig von der Größe des Datensatzes kann\n" +
                        " dies einige Zeit in Anspruch nehmen"
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
                int labelsDeletePosition = 0;

                string imageFileName = currentItem.ImageSourceFileName;

                if (imageFileName == "Image.png")
                {
                    labelsDeletePosition = 0;
                }
                else
                {
                    string imageFileNumber = "";

                    int imageIndex = 7;
                    while (imageFileName[imageIndex] != ')')
                    {
                        imageFileNumber += imageFileName[imageIndex];
                        imageIndex++;
                    }

                    labelsDeletePosition = Convert.ToInt32(imageFileNumber) * 2 - 2;
                }

                StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
                StorageFolder currentFolder = await datasetsFolder.GetFolderAsync((Lsv_Datasets.SelectedItem as Dataset).Name);

                List<string> allLabelsStringList = new List<string>();
                int allLabelsLength = 0;

                StorageFile initialLabelsFile = await currentFolder.TryGetItemAsync("Labels.txt") as StorageFile;


                if (initialLabelsFile != null)
                {
                    string newItems = await FileIO.ReadTextAsync(initialLabelsFile);
                    allLabelsStringList.Add(newItems);
                    allLabelsLength += newItems.Length;
                }

                int index = 2;
                while(allLabelsLength <= labelsDeletePosition + 1)
                {
                    StorageFile nextFile = await currentFolder.TryGetItemAsync($"Labels ({index}).txt") as StorageFile;
                    
                    if (nextFile == null)
                    {
                        break;
                    }

                    string newItems = await FileIO.ReadTextAsync(nextFile);

                    allLabelsStringList.Add(newItems);
                    allLabelsLength += newItems.Length;
                }

                string lastListItem = allLabelsStringList[allLabelsStringList.Count - 1];

                int deletePositionFromBack = allLabelsLength - labelsDeletePosition;
                int deletePositonFromBeginning = lastListItem.Length - deletePositionFromBack;

                lastListItem = lastListItem.Remove(deletePositonFromBeginning, 2);

                if (allLabelsStringList.Count == 1)
                {
                    await FileIO.WriteTextAsync(await currentFolder.GetFileAsync("Labels.txt"), lastListItem);
                }
                else
                {
                    await FileIO.WriteTextAsync(await currentFolder.GetFileAsync(
                        $"Labels ({allLabelsStringList.Count}).txt"), lastListItem);
                }

                StorageFile imageFile = await currentFolder.GetFileAsync(imageFileName);
                await imageFile.DeleteAsync();

                if (imageFileName == "Image.png")
                {
                    StorageFile newFirstFile = await currentFolder.TryGetItemAsync("Image (2).png") as StorageFile;
                    if (newFirstFile != null)
                    {
                        await newFirstFile.RenameAsync("Image.png");
                    }

                    int deleteSpecialTreatmentIndex = 3;

                    StorageFile nextDeleteSpecialTreatmentFile = await currentFolder.TryGetItemAsync(
                            $"Image ({deleteSpecialTreatmentIndex}).png") as StorageFile;

                    while (nextDeleteSpecialTreatmentFile != null)
                    {
                        await nextDeleteSpecialTreatmentFile.RenameAsync($"Image ({deleteSpecialTreatmentIndex - 1}).png");
                        deleteSpecialTreatmentIndex++;
                        nextDeleteSpecialTreatmentFile = await currentFolder.TryGetItemAsync(
                            $"Image ({deleteSpecialTreatmentIndex}).png") as StorageFile;
                    }
                }
                else
                {
                    string deleteIndexString = "";
                    int deleteIndexIntIterator = 7;

                    while (imageFileName[deleteIndexIntIterator] != ')')
                    {
                        deleteIndexString += imageFileName[deleteIndexIntIterator];
                        deleteIndexIntIterator++;
                    }

                    int deleteIndexInt = Convert.ToInt32(deleteIndexString);

                    StorageFile nextDeleteFile = await currentFolder.TryGetItemAsync(
                            $"Image ({deleteIndexInt}).png") as StorageFile;

                    while (nextDeleteFile != null)
                    {
                        await nextDeleteFile.RenameAsync($"Image ({deleteIndexInt - 1}).png");
                        nextDeleteFile = await currentFolder.TryGetItemAsync($"Image ({deleteIndexInt}).png")
                            as StorageFile;
                        deleteIndexInt++;
                    }
                }
            }
        }
    }
}
