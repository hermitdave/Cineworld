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
using Windows.Phone.System.UserProfile;
using Microsoft.Phone.Tasks;
using Windows.ApplicationModel.Store;

namespace CineWorld
{
    public partial class ConfigSettings : PhoneApplicationPage
    {
        public ConfigSettings()
        {
            InitializeComponent();
        }

        bool bLoaded = false;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Config.ShowSettings)
                Config.ShowSettings = false;

            if(App.IsFree)
            {
                if (App.ListingInfo == null)
                {
                    try
                    {
                        App.ListingInfo = await CurrentApp.LoadListingInformationAsync();
                    }
                    catch{}
                }

                if(App.ListingInfo != null && App.ListingInfo.ProductListings.ContainsKey(App.AdFreeIAP))
                {
                    var iapListing = App.ListingInfo.ProductListings[App.AdFreeIAP];
                    btnRemoveAds.Content = String.Format("{0} - {1}", btnRemoveAds.Content, iapListing.FormattedPrice);
                    btnRemoveAds.Visibility = System.Windows.Visibility.Visible;
                }

            }
            else
            {
                btnRemoveAds.Visibility = System.Windows.Visibility.Collapsed;
            }
            
            this.lbRegion.SelectionChanged -= lbRegion_SelectionChanged;

            this.lbRegion.SelectedIndex = (Config.Region == Config.RegionDef.UK ? 0 : 1);

            this.lbRegion.SelectionChanged += lbRegion_SelectionChanged;

            this.btnLocationServices.IsChecked = Config.UseLocation;

            if (!LockScreenManager.IsProvidedByCurrentApplication)
            {
                Config.AnimateLockscreen = false;
            }

            this.btnLockscreen.IsChecked = Config.AnimateLockscreen;

            this.btnTileAnimation.IsChecked = Config.AnimateTiles;

            this.btnGroupData.IsChecked = Config.GroupData;

            this.btnInAppNavToMobileWeb.IsChecked = Config.UseMobileWeb;

            this.lbTheme.SelectionChanged -= lbTheme_SelectionChanged;

            this.lbTheme.SelectedIndex = (Config.Theme == Config.ApplicationTheme.Dark ? 0 : 1);

            this.lbTheme.SelectionChanged += lbTheme_SelectionChanged;

            this.btnCleanBackground.IsChecked = Config.ShowCleanBackground;

            this.btnAudioSupport.IsChecked = Config.AudioSupport;
        }

        void lbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Config.Theme != (this.lbTheme.SelectedIndex == 0 ? Cineworld.Config.ApplicationTheme.Dark : Cineworld.Config.ApplicationTheme.Light))
            {
                Config.Theme = this.lbTheme.SelectedIndex == 0 ? Cineworld.Config.ApplicationTheme.Dark : Cineworld.Config.ApplicationTheme.Light;

                MessageBoxResult res = MessageBox.Show("App needs to be restarted to change the theme. Exit application now?", "App settings", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.OK)
                {
                    Application.Current.Terminate();
                }
            }
        }

        void lbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Region = (Config.RegionDef)this.lbRegion.SelectedIndex;
        }

        private void btnLocationServices_Click(object sender, RoutedEventArgs e)
        {
            Config.UseLocation = this.btnLocationServices.IsChecked == true;
        }

        private async void btnLockscreen_Click(object sender, RoutedEventArgs e)
        {
            Config.AnimateLockscreen = this.btnLockscreen.IsChecked == true;

            if (Config.AnimateLockscreen)
            {

                if (!LockScreenManager.IsProvidedByCurrentApplication)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    await LockScreenManager.RequestAccessAsync();
                }
            }
            else
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
            }

            Config.AnimateLockscreen = LockScreenManager.IsProvidedByCurrentApplication;
            
            this.btnLockscreen.IsChecked = LockScreenManager.IsProvidedByCurrentApplication;
        }

        private void btnTileAnimation_Click(object sender, RoutedEventArgs e)
        {
            Config.AnimateTiles = this.btnTileAnimation.IsChecked == true;
        }

        private void btnInAppNavToMobileWeb_Click(object sender, RoutedEventArgs e)
        {
            Config.UseMobileWeb = this.btnInAppNavToMobileWeb.IsChecked == true;
        }

        private void btnCleanBackground_Click(object sender, RoutedEventArgs e)
        {
            Config.ShowCleanBackground = this.btnCleanBackground.IsChecked == true;
        }

        private void btnAudioSupport_Click(object sender, RoutedEventArgs e)
        {
            Config.AudioSupport = this.btnAudioSupport.IsChecked == true;
        }

        private void HyperlinkButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask() { Uri = new Uri("http://www.invokeit.co.uk/cineworldprivacypolicy", UriKind.Absolute) };
            wbt.Show();
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private async void btnRemoveAds_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);
        }

        private void btnGroupData_Click(object sender, RoutedEventArgs e)
        {
            Config.GroupData = this.btnGroupData.IsChecked == true;
        }
    }
}