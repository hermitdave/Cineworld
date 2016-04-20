using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CineWorld;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel;
using Microsoft.Phone.Tasks;

namespace Cineworld
{
    public partial class BannerControl : UserControl
    {
        private static bool? _isWindows10;
        public static bool IsWindows10()
        {
            if (!_isWindows10.HasValue)
            {
                _isWindows10 = Package.Current.GetType().GetRuntimeProperty("Status") != null;
            }
            return _isWindows10.Value;
        }

        public BannerControl()
        {
            InitializeComponent();

            this.btnHub.IsFrozen = !App.IsFree;

            if (App.IsFree)
            {
                if (IsWindows10() && !Config.MolAdTapped)
                {
                    this.btnMOLAd.Tap -= BtnMOLAd_Tap;
                    this.btnMOLAd.Tap += BtnMOLAd_Tap;

                    this.btnMOLAd.Tap -= LogMOLAd_Tap;
                    this.btnMOLAd.Tap += LogMOLAd_Tap;

                    this.btnMOLAd.Visibility = Visibility.Visible;
                    this.btnMOLAd.IsFrozen = false;

                }
                else
                {
                    this.adControl.Visibility = System.Windows.Visibility.Visible;
                }

                this.btnHub.Tap -= btnHub_Tap;
                this.btnHub.Tap += btnHub_Tap;
            }
            else
            {
                this.btnMOLAd.Visibility = Visibility.Collapsed;

                this.adControl.IsEnabled = false;
                this.adControl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void BtnMOLAd_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string url = "https://www.microsoft.com/store/apps/9nblggh67jjh";
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri(url, UriKind.Absolute);

            wbt.Show();
        }

        private async void LogMOLAd_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                BookingHistory bh = new BookingHistory();

                bh.UserId = ExtendedPropertyHelper.GetUserIdentifier();

                await App.MobileService.GetTable<BookingHistory>().InsertAsync(bh);

                Config.MolAdTapped = true;
            }
            catch { }
        }

        public static readonly DependencyProperty ViewTitleProperty = DependencyProperty.RegisterAttached("ViewTitle", typeof(String), typeof(BannerControl), new PropertyMetadata("", OnViewTitleChanged));
        private object Launcher;

        private static void OnViewTitleChanged(DependencyObject theTarget, DependencyPropertyChangedEventArgs e)
        {
            BannerControl bc = theTarget as BannerControl;
            if (bc != null)
            {
                string value = e.NewValue as string;
                if (String.IsNullOrEmpty(value))
                {
                    bc.tbTitle.Text = String.Empty;
                    bc.tbTitle.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bc.tbTitle.Text = value;
                    bc.tbTitle.Visibility = Visibility.Visible;
                }
            }
        }
        public string ViewTitle
        {
            get { return GetValue(ViewTitleProperty).ToString(); }
            set { SetValue(ViewTitleProperty, value); }
        }

        private void adControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {

        }

        private async void btnRemoveAds_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);
            }
            catch { }
        }

        private async void btnHub_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);
            }
            catch { }
        }
    }
}
