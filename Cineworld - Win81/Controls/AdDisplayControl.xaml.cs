using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Cineworld.Controls
{
    public sealed partial class AdDisplayControl : UserControl
    {
        public AdDisplayControl()
        {
            this.InitializeComponent();

            InitialiseView();
        }

        private void InitialiseView()
        {
            Size res = GetDisplaySize();

            this.ad160x600.IsEnabled = this.ad300x600.IsEnabled = App.IsFree;

            if (App.IsFree)
            {
                if (App.ListingInfo != null && App.ListingInfo.ProductListings != null && App.ListingInfo.ProductListings.ContainsKey(App.AdFreeIAP))
                {
                    this.tbRemoveAds.Text = String.Format("{0} - {1}", this.tbRemoveAds.Text, App.ListingInfo.ProductListings[App.AdFreeIAP].FormattedPrice);
                }

                this.btnRemoveAds.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (res.Width > 1280)
                {
                    this.ad300x600.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.btnRemoveAds.Width = this.ad300x600.Width;
                }
                else
                {
                    this.ad160x600.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.btnRemoveAds.Width = this.ad160x600.Width;
                }
            }
            else
            {
                this.btnRemoveAds.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.ad160x600.Visibility = this.ad300x600.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private Size GetDisplaySize()
        {
            var bounds = Window.Current.Bounds;

            double w = bounds.Width;

            double h = bounds.Height;

            switch (DisplayProperties.ResolutionScale)
            {

                case ResolutionScale.Scale140Percent:

                    w = Math.Ceiling(w * 1.4);

                    h = Math.Ceiling(h * 1.4);

                    break;

                case ResolutionScale.Scale180Percent:

                    w = Math.Ceiling(w * 1.8);

                    h = Math.Ceiling(h * 1.8);

                    break;

            }

            Size resolution = new Size(w, h);

            return resolution;
        }

        private async void btnRemoveAds_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);

                if (CurrentApp.LicenseInformation != null && CurrentApp.LicenseInformation.ProductLicenses != null && CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(App.AdFreeIAP))
                {
                    App.IsFree = !CurrentApp.LicenseInformation.ProductLicenses[App.AdFreeIAP].IsActive;
                    this.InitialiseView();
                }
            }
            catch { }
        }
    }
}
