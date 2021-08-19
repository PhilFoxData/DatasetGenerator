using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
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
    public sealed partial class Settings : Page
    {

        public Settings()
        {
            this.InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            string savedTheme = "";
            savedTheme = ApplicationData.Current.LocalSettings.Values[ThemeHelper.SelectedAppThemeKey] as string;

            switch (savedTheme)
            {
                case "Default":
                    Cbx_ColorMode.SelectedIndex = 0;
                    break;
                case "Light":
                    Cbx_ColorMode.SelectedIndex = 1;
                    break;
                case "Dark":
                    Cbx_ColorMode.SelectedIndex = 2;
                    break;
                default:
                    Cbx_ColorMode.SelectedIndex = 0;
                    break;
            }
        }

        private void Cbx_ColorMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTheme;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            
            
            switch (e.AddedItems[0])
            {
                case "Hell":
                    selectedTheme = "Light";
                    break;
                case "Dunkel":
                    selectedTheme = "Dark";
                    break;
                case "Systemstandard verwenden":
                    selectedTheme = "Default";
                    break;
                default:
                    selectedTheme = "Default";
                    break;
            }

            if (selectedTheme != null)
            {
                ThemeHelper.RootTheme = App.GetEnum<ElementTheme>(selectedTheme);
            }
        }

    }
}
