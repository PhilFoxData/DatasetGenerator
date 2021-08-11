using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace DatasetGenerator
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class NewDataEditor : Page
    {
        public NewDataEditor()
        {
            InitializeComponent();
        }

        string Label = "";
        Random random = new Random(DateTime.Now.Millisecond);
        int lastRandomNumber = -1;

        private void SetNewLabel()
        {
            int newNumber = random.Next(0, 10);

            while (newNumber == lastRandomNumber)
            {
                newNumber = random.Next(0, 10);
            }

            lastRandomNumber = newNumber;

            Label = newNumber.ToString();
            Txb_Label.Text = "Zeichne eine: " + Label;
        }

        private void Cmd_Confirm_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> strokes = ICv_Image.InkPresenter.StrokeContainer.GetStrokes();

            int canvasHeight = (int)ICv_Image.ActualHeight;
            int canvasWidth = (int)ICv_Image.ActualWidth;

            var device = CanvasDevice.GetSharedDevice();

            CanvasRenderTarget crt = new CanvasRenderTarget(device, canvasWidth, canvasHeight, canvasHeight);

            using (var ds = crt.CreateDrawingSession())
            {
                ds.Clear(Colors.Black);
                ds.DrawInk(strokes);
            }

            Transmitter.CurrentlyEditedDataset.ImageData.Add(crt.GetPixelBytes().AsBuffer());

            Transmitter.CurrentlyEditedDataset.NewLabels.Add(Label);

            Transmitter.CurrentlyEditedDataset.NewRenderTargetsSize = (int)crt.SizeInPixels.Height;

            Transmitter.CurrentlyEditedDataset.CheckForDataSession();

            ICv_Image.InkPresenter.StrokeContainer.Clear();

            SetNewLabel();

            Cmd_Confirm.IsEnabled = false;
        }

        private void Cmd_Stop_Click(object sender, RoutedEventArgs e)
        {
            Transmitter.CurrentlyEditedDataset.SaveRemainingNewData();
            Frame.Navigate(typeof(Home));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetNewLabel();
        }

        private void ICv_Image_Loaded(object sender, RoutedEventArgs e)
        {
            ICv_Image.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            Cmd_Confirm.IsEnabled = true;
        }
    }
}
