using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Reflection;

namespace CineWorld
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void btnRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceReviewTask mrt = new MarketplaceReviewTask();
                mrt.Show();
            }
            catch (InvalidOperationException) { }
        }

        private void btnShowApps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceSearchTask mst = new MarketplaceSearchTask();
                mst.SearchTerms = "invoke it limited";
                mst.Show();
            }
            catch (InvalidOperationException) { }
        }

        private void Facebook_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri facebookUri = new Uri("http://touch.facebook.com/pages/CineworldMetro/477303595629118", UriKind.Absolute);
                WebBrowserTask wbt = new WebBrowserTask() { Uri = facebookUri };
                wbt.Show();
            }
            catch (InvalidOperationException) { }
        }

        private void Twitter_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri twitterUri = new Uri("http://mobile.twitter.com/CineworldMetro", UriKind.Absolute);
                WebBrowserTask wbt = new WebBrowserTask() { Uri = twitterUri };
                wbt.Show();
            }
            catch (InvalidOperationException) { }
        }

        private void Email_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EmailComposeTask ect = new EmailComposeTask();
                ect.To = "info@invokeit.co.uk";

                ect.Subject = "Cineworld " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

                ect.Show();
            }
            catch (InvalidOperationException) { }
        }

        private void btnContactus_Click(object sender, RoutedEventArgs e)
        {
            cmContactUs.IsOpen = true;
        }
    }
}