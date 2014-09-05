using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CineWorld
{
    public partial class InAppBrowser : PhoneApplicationPage
    {
        //string useragent = "User-Agent: Mozilla/4.0 (Compatible; MSIE 7.0; Windows NT 6.2; WOW64; Trident/6.0)";
        string loginUrl = "https://www.cineworld.co.uk/mobile/login";

        public InAppBrowser()
        {
            InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.scWaiting.IsSpinning = bNewVal;
            this.wbCineworld.IsEnabled = !bNewVal;
            this.wbCineworld.Opacity = (bNewVal ? 0 : 1);
        }
 
        public static Uri NavigationUri { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.wbCineworld.Navigating += wbCineworld_Navigating;
            
            this.wbCineworld.Navigated += wbCineworld_Navigated;
            this.wbCineworld.Navigate(NavigationUri);//, null, useragent);

            
        }

        void wbCineworld_Navigating(object sender, NavigatingEventArgs e)
        {
            this.SpinAndWait(true);
        }

        //private Uri m_UriTmp;
        //private void wbCineworld_Navigating(object sender, NavigatingEventArgs e)
        //{
        //    if (null != e.Uri && !e.Uri.AbsoluteUri.StartsWith(loginUrl))
        //    {
        //        if (!e.Uri.Equals(m_UriTmp))
        //        {
        //            m_UriTmp = e.Uri;
        //            e.Cancel = true;

        //            this.Dispatcher.BeginInvoke(() => wbCineworld.Navigate(m_UriTmp, null, useragent));
        //        }
        //    }
        //}

        void wbCineworld_Navigated(object sender, NavigationEventArgs e)
        {
            this.SpinAndWait(false);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.wbCineworld.CanGoBack)
            {
                this.wbCineworld.GoBack();
                e.Cancel = true;
            }
        }

        private void btnExitTicketing_Click(object sender, EventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}