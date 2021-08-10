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

        private async void WriteOutData( List<string> clonedLabels, List<IBuffer> clonedImageData)
        {
            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");
            StorageFolder currentDSFolder = await datasetsFolder.GetFolderAsync(Name);
            StorageFile NewLabelsFile = await currentDSFolder.CreateFileAsync("Labels.txt", CreationCollisionOption.GenerateUniqueName);

            string LabelsString = "";

            for (int i = 0; i < clonedLabels.Count; i++)
            {
                LabelsString += clonedLabels[i] + ";";
            }

            await FileIO.WriteTextAsync(NewLabelsFile, LabelsString);

            for (int i = 0; i < clonedImageData.Count; i++)
            {
                StorageFile NewImageFile = await currentDSFolder.CreateFileAsync("Image.png", CreationCollisionOption.GenerateUniqueName);

                using (var stream = await NewImageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(clonedImageData[i], BitmapPixelFormat.Bgra8,
                            NewRenderTargetsSize, NewRenderTargetsSize);

                    BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                    bitmapEncoder.SetSoftwareBitmap(softwareBitmap);

                    bitmapEncoder.BitmapTransform.ScaledHeight = 50;
                    bitmapEncoder.BitmapTransform.ScaledWidth = 50;

                    await bitmapEncoder.FlushAsync();
                    await stream.FlushAsync();
                }
            }
        }

        public async Task CreateDataset()
        {
            StorageFolder datasetFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Datasets");

            StorageFolder newRootfolder = await datasetFolder.CreateFolderAsync(Name);

            StorageFile jsonFile = await newRootfolder.CreateFileAsync("Dataset_Info.json");

            string jsonString = JsonSerializer.Serialize(this);

            await FileIO.WriteTextAsync(jsonFile, jsonString);
        }
    }
}
