using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Storage;
using System.Text.Json;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace DatasetGenerator
{
    public class Dataset : DependencyObject
    {
        public Dataset()
        {
            NewLabels = new List<string>();
            ImageData = new List<IBuffer>();
            NamesOfFiles = new List<string>[10];

            for (int i = 0; i < 10; i++)
            {
                NamesOfFiles[i] = new List<string>();
            }
        }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(Dataset), new PropertyMetadata("Unnamed dataset"));



        public string Discription
        {
            get { return (string)GetValue(DiscriptionProperty); }
            set { SetValue(DiscriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Discription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DiscriptionProperty =
            DependencyProperty.Register("Discription", typeof(string), typeof(Dataset), new PropertyMetadata("No discription available"));

        public double DesiredLength { get; set; }
        public double CurrentLength { get; set; }

        public List<string> NewLabels { get; set; }
        public List<IBuffer> ImageData { get; set; }
        public int NewRenderTargetsSize { get; set; }

        public int ImageResolution { get; set; }

        public List<string>[] NamesOfFiles { get; set; }

        public void SaveRemainingNewData()
        {
            List<string> clonedLabels = new List<string>(NewLabels);
            List<IBuffer> clonedImageData = new List<IBuffer>(ImageData);

            NewLabels.Clear();
            ImageData.Clear();

            WriteOutData(clonedLabels, clonedImageData);
        }

        public void CheckForDataSession()
        {
            if (NewLabels.Count >= 10 || ImageData.Count >= 10)
            {
                List<string> clonedLabels = new List<string>(NewLabels);
                List<IBuffer> clonedImageData = new List<IBuffer>(ImageData);

                NewLabels.Clear();
                ImageData.Clear();
                
                WriteOutData(clonedLabels, clonedImageData);
            }
        }

        private async void WriteOutData(List<string> clonedLabels, List<IBuffer> clonedImageData)
        {
            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            StorageFolder currentDatasetFolder = await datasetsFolder.GetFolderAsync(Name);

            for (int i = 0; i < clonedLabels.Count; i++)
            {
                StorageFolder currentImageFolder = await currentDatasetFolder.GetFolderAsync(clonedLabels[i]);

                StorageFile newFile = await currentImageFolder.CreateFileAsync("Image.png",
                        CreationCollisionOption.GenerateUniqueName);

                NamesOfFiles[Convert.ToInt32(clonedLabels[i])].Add(newFile.Name);


                using (var stream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(clonedImageData[i],
                            BitmapPixelFormat.Bgra8, NewRenderTargetsSize, NewRenderTargetsSize);

                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId,
                            stream);

                    encoder.SetSoftwareBitmap(softwareBitmap);

                    encoder.BitmapTransform.ScaledHeight = 50;
                    encoder.BitmapTransform.ScaledWidth = 50;

                    await encoder.FlushAsync();
                    await stream.FlushAsync();
                }
            }

            StorageFile jsonFile = await currentDatasetFolder.GetFileAsync("Dataset_Info.json");
            await FileIO.WriteTextAsync(jsonFile, JsonSerializer.Serialize(this));
        }

        public async Task CreateDataset()
        {
            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            
            StorageFolder newDatasetfolder = await datasetsFolder.CreateFolderAsync(Name);
            
            StorageFile jsonFile = await newDatasetfolder.CreateFileAsync("Dataset_Info.json");
            
            for (int i = 0; i < 10; i++)
            {
                await newDatasetfolder.CreateFolderAsync(i.ToString());
            }

            string jsonString = JsonSerializer.Serialize(this);

            await FileIO.WriteTextAsync(jsonFile, jsonString);
        }
    }
}
