using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using Cineworld;
using System.Device.Location;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media.Imaging;
using Cineworld.Services;
using System.IO.IsolatedStorage;
using System.IO;

namespace CineWorld
{
    public partial class ListCinemas : PhoneApplicationPage
    {
        bool bLoaded = false;
        
        public ListCinemas()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (bLoaded)
                return;

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("SplashScreenImage-WVGA.jpg", UriKind.Relative)),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            LoadPushpins();

            //using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    using (FileStream fs = isf.OpenFile("cinemadetails.csv", FileMode.Create))
            //    {
            //        using (StreamWriter sw = new StreamWriter(fs))
            //        {
            //            sw.WriteLine("id, postcode, longitude, latitude");

            //            foreach (var entry in App.Cinemas.Values)
            //            {
            //                sw.WriteLine(String.Format("{0}, {1}, {2}, {3}", entry.ID, entry.Postcode, entry.Longitute, entry.Latitude));
            //            }
            //        }
            //    }
            //}

            CinemaData cd = new CinemaData(App.Cinemas);

            var dataLetter = cd.GetGroupsByLetter();

            this.lstMain.ItemsSource = dataLetter.ToList();

            bLoaded = true;
        }

        private void LoadPushpins()
        {
            GeoCoordinate location = null;

            if (Config.UseLocation && Landing.userPosition != null)
            {
                location = Landing.userPosition;
                mapView.ZoomLevel = 12;
            }
            else
            {
                mapView.ZoomLevel = 5;

                if (Config.Region == Config.RegionDef.UK)
                    location = new GeoCoordinate(51.507222, -0.1275);
                else
                    location = new GeoCoordinate(53.347778, -6.259722);
            }

            IEnumerable<CinemaInfo> oc = App.Cinemas.Values.OrderBy(c => GeoMath.Distance(location.Latitude, location.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(1);
            if (oc.Count() > 0)
            {
                CinemaInfo ci = oc.First();
                mapView.Center = new GeoCoordinate(ci.Latitude, ci.Longitute);
            }

            MapLayer layer = new MapLayer();
            
            foreach (var cinema in App.Cinemas.Values)
            {
                Pushpin pin = new Pushpin
                {
                    GeoCoordinate = cinema.Location,
                    Content = cinema.Name,
                    Tag = cinema
                };

                pin.Tap += Cinema_Tap;

                MapOverlay overlay = new MapOverlay();
                overlay.Content = pin;
                overlay.GeoCoordinate = cinema.Location;
                overlay.PositionOrigin = new Point(0, 1);

                layer.Add(overlay);
            }

            this.mapView.Layers.Add(layer);
        }

        //void pin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    CinemaInfo ci = (CinemaInfo)(sender as Pushpin).Tag;

        //    if (ci != null)
        //    {
        //        CinemaDetails.SelectedCinema = ci;
        //        NavigationService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
        //    }
        //}

        //private void llsCinemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (this.lstMain.SelectedItem != null)
        //    {
        //        CinemaDetails.SelectedCinema = (CinemaInfo)this.lstMain.SelectedItem;
        //        this.lstMain.SelectionChanged -= new SelectionChangedEventHandler(llsCinemas_SelectionChanged);
        //        this.lstMain.SelectedItem = null;
        //        this.lstMain.SelectionChanged += new SelectionChangedEventHandler(llsCinemas_SelectionChanged);
        //        NavigationService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
        //    }
        //}

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        CinemaInfo ci = null;

        private async void Cinema_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            CinemaInfo newCI = null;
            if (sender is TextBlock)
            {
                TextBlock tb = (sender as TextBlock);
                newCI = tb.Tag as CinemaInfo;
            }
            else
            {
                Pushpin pp = (sender as Pushpin);
                newCI = pp.Tag as CinemaInfo;
            }

            if (Config.AudioSupport && ci != newCI)
            {
                ci = newCI;

                await SpeechSynthesisService.SpeakOutLoud(newCI.Name);

                return;
            }
            else
            {
                CinemaDetails.SelectedCinema = newCI;
                NavigationService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
            }
        }
    }
}