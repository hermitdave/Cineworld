using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Cineworld;
using Microsoft.Phone.Tasks;

namespace CineWorld
{
    public partial class ConfigSettings : PhoneApplicationPage
    {
        public ConfigSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Config.ShowSettings)
                Config.ShowSettings = false;

            this.lbRegion.SelectionChanged -= lbRegion_SelectionChanged;

            this.lbRegion.SelectedIndex = (Config.Region == Config.RegionDef.UK ? 0 : 1);

            this.lbRegion.SelectionChanged += lbRegion_SelectionChanged;

            this.btnLocationServices.IsChecked = Config.UseLocation;

            this.btnTileAnimation.IsChecked = Config.AnimateTiles;

            this.btnInAppNavToMobileWeb.IsChecked = Config.UseMobileWeb;

            this.lbTheme.SelectionChanged -= lbTheme_SelectionChanged;

            this.lbTheme.SelectedIndex = (Config.Theme == Config.ApplicationTheme.Dark ? 0 : 1);

            this.lbTheme.SelectionChanged += lbTheme_SelectionChanged;

            this.btnCleanBackground.IsChecked = Config.ShowCleanBackground;
        }

        void lbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Config.Theme != (this.lbTheme.SelectedIndex == 0 ? Cineworld.Config.ApplicationTheme.Dark : Cineworld.Config.ApplicationTheme.Light))
            {
                Config.Theme = this.lbTheme.SelectedIndex == 0 ? Cineworld.Config.ApplicationTheme.Dark : Cineworld.Config.ApplicationTheme.Light;

                MessageBoxResult res = MessageBox.Show("App needs to be restarted to change the theme.", "App settings", MessageBoxButton.OK);
            }
        }

        private void btnCleanBackground_Click(object sender, RoutedEventArgs e)
        {
            Config.ShowCleanBackground = this.btnCleanBackground.IsChecked == true;
        }


        void lbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Region = (Config.RegionDef)this.lbRegion.SelectedIndex;
        }

        private void btnLocationServices_Click(object sender, RoutedEventArgs e)
        {
            Config.UseLocation = this.btnLocationServices.IsChecked == true;
        }        

        private void btnTileAnimation_Click(object sender, RoutedEventArgs e)
        {
            Config.AnimateTiles = this.btnTileAnimation.IsChecked == true;
        }

        private void btnInAppNavToMobileWeb_Click(object sender, RoutedEventArgs e)
        {
            Config.UseMobileWeb = this.btnInAppNavToMobileWeb.IsChecked == true;
        }

        private void HyperlinkButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask() { Uri = new Uri("http://www.invokeit.co.uk/cineworldprivacypolicy", UriKind.Absolute) };
            wbt.Show();
        }
    }
}