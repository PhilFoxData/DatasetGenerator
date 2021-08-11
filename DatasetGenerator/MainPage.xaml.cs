using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.Text.Json;
using Windows.Storage.Pickers;
using System.Threading.Tasks;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace DatasetGenerator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                Frm_Content.Navigate(typeof(Settings));
                return;
            }

            switch (args.InvokedItemContainer.Tag.ToString())
            {
                case "Nvi_Home":
                    _ = Frm_Content.Navigate(typeof(Home));
                    break;
                case "Nvi_Generate":
                    _ = Frm_Content.Navigate(typeof(GenerateData));
                    break;
                case "Nvi_Manage":
                    _ = Frm_Content.Navigate(typeof(ManageData));
                    break;
            }            
        }

        private async void Page_Loaded(object _, RoutedEventArgs _1)
        {
            Frm_Content.Navigate(typeof(Home));

            StorageFolder rootFolder = ApplicationData.Current.LocalFolder;

            IStorageItem datasetsFolder = await rootFolder.TryGetItemAsync("Datasets");

            if (datasetsFolder != null)
            {
                List<Dataset> datasets = new List<Dataset>();

                foreach (var item in await (datasetsFolder as StorageFolder).GetFoldersAsync())
                {
                    StorageFile Dataset_Info = await item.GetFileAsync("Dataset_Info.json");

                    string jsonString = await FileIO.ReadTextAsync(Dataset_Info);

                    Dataset current_dataset = JsonSerializer.Deserialize<Dataset>(jsonString);

                    datasets.Add(current_dataset);
                }

                Transmitter.Datasets = datasets;
            }
            else
            {
                await rootFolder.CreateFolderAsync("Datasets");
                Transmitter.Datasets = new List<Dataset>();
            }

            IStorageItem settingsFile = await rootFolder.TryGetItemAsync("Settings.json");
            
            if (settingsFile != null)
            {
                string jsonString = await FileIO.ReadTextAsync(settingsFile as StorageFile);

                Transmitter.Settings = JsonSerializer.Deserialize<DatasetGeneratorSettings>(jsonString);
            }
            else
            {
                StorageFile newSettingsFile = await rootFolder.CreateFileAsync("Settings.json");
                Transmitter.Settings = new DatasetGeneratorSettings();

                string initialJsonString = JsonSerializer.Serialize(new DatasetGeneratorSettings());

                await FileIO.WriteTextAsync(newSettingsFile, initialJsonString);
            }
        }

        // >>>>> DEVELOPMENT UTILITIES <<<<<

        //This code is used for development purpose only, don't include it in conventional code

        static async Task DeleteAllData()
        {
            StorageFolder rootFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            foreach (var item in await rootFolder.GetItemsAsync())
            {
                await item.DeleteAsync();
            }

            Application.Current.Exit();
        }

        static async void ReadAllInternalData()
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            StorageFolder zielOrdner = await picker.PickSingleFolderAsync();

            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            StorageFolder sourceFolder = await datasetsFolder.GetFolderAsync("Numeric_100K");

            foreach(var item in await sourceFolder.GetItemsAsync())
            {
                await (item as StorageFile).CopyAsync(zielOrdner);
            }

            Application.Current.Exit();
        }
    }
}
