using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Cineworld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.IsFree)
            {
                if (App.ListingInfo == null)
                {
                    try
                    {
                        App.ListingInfo = await CurrentApp.LoadListingInformationAsync();
                    }
                    catch { }
                }

                if (App.ListingInfo != null && App.ListingInfo.ProductListings.ContainsKey(App.AdFreeIAP))
                {
                    var iapListing = App.ListingInfo.ProductListings[App.AdFreeIAP];
                    btnRemoveAds.Content = String.Format("{0} - {1}", btnRemoveAds.Content, iapListing.FormattedPrice);
                    btnRemoveAds.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }

            }
            else
            {
                btnRemoveAds.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Region = (Config.RegionDef)this.cbRegion.SelectedIndex;
        }

        private void Page_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.tbRegion.Visibility = this.cbRegion.Visibility = Config.ShowRegion ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            
            this.cbRegion.SelectionChanged -= cbRegion_SelectionChanged;

            this.cbRegion.SelectedIndex = (Config.Region == Config.RegionDef.UK ? 0 : 1);

            this.cbRegion.SelectionChanged += cbRegion_SelectionChanged;

            this.cbQuality.SelectionChanged -= cbQuality_SelectionChanged;

            this.cbQuality.SelectedIndex =  Config.GetIntFromYoutubeQuality(Config.TrailerQuality);

            this.cbQuality.SelectionChanged += cbQuality_SelectionChanged;

            this.btnInAppNavToWeb.IsOn = Config.UseMobileWeb;

            this.btnInAppNavToWeb.Toggled += btnInAppNavToWeb_Toggled;

            this.cbTheme.SelectionChanged -= cbTheme_SelectionChanged;

            this.cbTheme.SelectedIndex = Config.Theme == ApplicationTheme.Dark ? 0 : 1;

            this.cbTheme.SelectionChanged += cbTheme_SelectionChanged;

            this.btnShowCleanBackground.Toggled += btnShowCleanBackground_Toggled;
            this.btnShowCleanBackground.IsOn = Config.ShowCleanBackground;
        }

        void btnShowCleanBackground_Toggled(object sender, RoutedEventArgs e)
        {
            Config.ShowCleanBackground = this.btnShowCleanBackground.IsOn;
        }

        async void cbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Theme = this.cbTheme.SelectedIndex == 0 ? ApplicationTheme.Dark : ApplicationTheme.Light;

            MessageDialog md = new MessageDialog("App needs to be restarted to change the theme. Exit application now?", "App settings");
            bool? restartAppResult = null;
            md.Commands.Add(new Windows.UI.Popups.UICommand("OK", new Windows.UI.Popups.UICommandInvokedHandler((cmd) => restartAppResult = true)));
            md.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", new Windows.UI.Popups.UICommandInvokedHandler((cmd) => restartAppResult = false)));
            await md.ShowAsync();
            if (restartAppResult == true)
            {
                Application.Current.Exit();
            }                
        }

        void btnInAppNavToWeb_Toggled(object sender, RoutedEventArgs e)
        {
            Config.UseMobileWeb = this.btnInAppNavToWeb.IsOn;
        }

        void cbQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbQuality.SelectedIndex > -1)
                Config.TrailerQuality = Config.GetYoutubeQualityFromInt(this.cbQuality.SelectedIndex);
        }

        private async void btnRemoveAds_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);
             
            if(CurrentApp.LicenseInformation != null && CurrentApp.LicenseInformation.ProductLicenses != null && CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(App.AdFreeIAP))
            {
                App.IsFree = !CurrentApp.LicenseInformation.ProductLicenses[App.AdFreeIAP].IsActive;
                if (!App.IsFree)
                {
                    this.btnRemoveAds.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }
    }
}
