using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using System.Globalization;
using MyToolkit.Multimedia;
using Cineworld;
using Coding4Fun.Toolkit.Controls;

namespace CineWorld
{
    public partial class ShowPerformances : PhoneApplicationPage
    {
        public static CinemaInfo SelectedCinema { get; set; }
        public static FilmInfo SelectedFilm { get; set; }
        
        public static bool ShowFilmDetails { get; set; }
        public static bool ShowCinemaDetails { get; set; }

        ApplicationBarIconButton abibPin = null;
        ApplicationBarIconButton abibAddFav = null;
        ApplicationBarIconButton abibRemFav = null;
        ApplicationBarIconButton abibDrive = null;

        ApplicationBarIconButton abibTrailer = null;
        ApplicationBarIconButton abibShare = null;

        bool bLoaded = false;

        public ShowPerformances()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.abibAddFav = new ApplicationBarIconButton() { Text = "favourite", IconUri = new Uri("/Images/appbar.favs.addto.rest.png", UriKind.Relative) };
            this.abibAddFav.Click += this.btnFavourite_Click;

            this.abibRemFav = new ApplicationBarIconButton() { Text = "unfavourite", IconUri = new Uri("/Images/appbar.favs.rem.rest.png", UriKind.Relative) };
            this.abibRemFav.Click += this.btnUnfavourite_Click;

            this.abibPin = new ApplicationBarIconButton() { Text = "pin", IconUri = new Uri("/Images/appbar.pin.png", UriKind.Relative) };
            this.abibPin.Click += this.btnPinSecondary_Click;

            this.abibTrailer = new ApplicationBarIconButton() { Text = "trailer", IconUri = new Uri("/Images/appbar.transport.play.rest.png", UriKind.Relative) };
            this.abibTrailer.Click += this.btnViewTrailer_Click;

            this.abibShare = new ApplicationBarIconButton() { Text = "share", IconUri = new Uri("/Images/appbar.share03.png", UriKind.Relative) };
            this.abibShare.Click += this.btnShare_Click;

            this.ApplicationBar.Buttons.Clear();

            if (!Config.ShowCleanBackground && SelectedFilm.BackdropUrl != null)
            {
                this.pMain.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(SelectedFilm.BackdropUrl),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            this.pMain.Title = String.Format("{0} at Cineworld {1}", SelectedFilm.TitleWithClassification, SelectedCinema.Name);

            this.DataContext = this;

            bool bError = false;
            try
            {
                if (!Config.ShowCleanBackground && SelectedFilm.MediumPosterUrl != null)
                {
                    this.pMain.Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(SelectedFilm.MediumPosterUrl),
                        Opacity = 0.2,
                        Stretch = Stretch.UniformToFill
                    };
                }

                if (ShowCinemaDetails)
                    await this.LoadCinemaDetails();
                
                if (ShowFilmDetails)
                    this.LoadFilmDetails();
                
                var query = from item in SelectedFilm.Performances
                            orderby ((PerformanceInfo)item).PerformanceTS
                            group item by ((PerformanceInfo)item).PerformanceTS.Date into g
                            select new Group<PerformanceInfo>(DateTimeToStringConverter.ConvertData(g.Key.Date), g);
            
                this.lbPerformances.ItemsSource = query;
            }
            catch
            {
                bError = true;
            }

            if (bError)
                MessageBox.Show("Error downloading showtime data");
            else
                bLoaded = true;

        }

        void btnShare_Click(object sender, EventArgs e)
        {
            SetContextMenu();
        }

        private void SetContextMenu()
        {
            string message = String.Format("Shall we go and see \"{0}\" at Cineworld {1}?", SelectedFilm.Title, SelectedCinema.Name);

             new AppBarPrompt(
                new AppBarPromptAction("sms", () =>
                {
                    new SmsComposeTask()
                    {
                        Body = message
                    }.Show();
                }),
                new AppBarPromptAction("email", () =>
                {
                    new EmailComposeTask()
                    {
                        Body = message
                    }.Show();
                }),
                new AppBarPromptAction("social networks", () =>
                {
                    new ShareStatusTask()
                    {
                        Status = message
                    }.Show();
                })
                ) { Foreground = new SolidColorBrush(Colors.White) }.Show();
        }

        private void LoadFilmDetails()
        {
            if(pMain.Items.Contains(piCinema))
                this.pMain.Items.Remove(piCinema);

            if (!String.IsNullOrWhiteSpace(SelectedFilm.YoutubeTrailer))
            {
                this.ApplicationBar.Buttons.Add(this.abibTrailer);
                this.btnPlay.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.btnPlay.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (SelectedFilm.FilmCast == null || SelectedFilm.FilmCast.Count == 0)
            {
                this.pMain.Items.Remove(this.piCast);
            }

            this.ApplicationBar.Buttons.Add(this.abibShare);
        }

        private void lbCast_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbCast = sender as ListBox;

            if (lbCast.SelectedItem != null)
            {
                PersonDetails.castinfo = (CastInfo)lbCast.SelectedItem;

                lbCast.SelectedIndex = -1;

                NavigationService.Navigate(new Uri("/PersonDetails.xaml", UriKind.Relative));
            }
        }

        private void filmRating_ValueChanged(object sender, EventArgs e)
        {
            RateAndReviewFilm();
        }

        private void RateAndReviewFilm()
        {
            Review.SelectedFilm = ShowPerformances.SelectedFilm;
            Review.ReviewTarget = Review.ReviewTargetDef.Film;

            NavigationService.Navigate(new Uri("/Review.xaml", UriKind.Relative));
        }

        private void bRateReviewFilm_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.RateAndReviewFilm();
        }

        private async Task LoadCinemaDetails()
        {
            this.DataContext = SelectedCinema;

            Task taskCinemaFilmListing = null;

            if (SelectedFilm.Performances == null || SelectedFilm.Performances.Count == 0)
            {
                taskCinemaFilmListing = new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID);
            }

            if(pMain.Items.Contains(piFilm))
                this.pMain.Items.Remove(piFilm);
            
            if(pMain.Items.Contains(piCast))
                this.pMain.Items.Remove(piCast);

            if (pMain.Items.Contains(piReviews))
                this.pMain.Items.Remove(piReviews);

            if(!pMain.Items.Contains(piCinema))
                pMain.Items.Add(piCinema);

            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(String.Format("CinemaID={0}", SelectedCinema.ID)));

            if (TileToFind == null)
                this.ApplicationBar.Buttons.Add(this.abibPin);

            if (Config.FavCinemas.Contains(SelectedCinema.ID))
                this.ApplicationBar.Buttons.Add(this.abibRemFav);
            else
                this.ApplicationBar.Buttons.Add(this.abibAddFav);

            this.ApplicationBar.Buttons.Add(this.abibShare);

            if (taskCinemaFilmListing != null)
            {
                await taskCinemaFilmListing;

                SelectedFilm = App.CinemaFilms[SelectedCinema.ID].First(f => f.EDI == SelectedFilm.EDI);
            }
        }

        private void cinemaRating_ValueChanged(object sender, EventArgs e)
        {
            RateAndReviewCinema();
        }

        private void bRateReviewCinema_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.RateAndReviewCinema();
        }

        private void RateAndReviewCinema()
        {
            Review.SelectedCinema = ShowPerformances.SelectedCinema;
            Review.ReviewTarget = Review.ReviewTargetDef.Cinema;

            NavigationService.Navigate(new Uri("/Review.xaml", UriKind.Relative));
        }

        static ContextMenu currentContextMenu = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Grid gParent = btn.Parent as Grid;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Tag = btn.CommandParameter;

            MenuItem miBuy = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "buy tickets" };
            miBuy.Click += this.BuyTickets_MenuItem_Click;
            miBuy.Click += this.LogBookingStats;
            contextMenu.Items.Add(miBuy);

            MenuItem miSpacer = new MenuItem();
            contextMenu.Items.Add(miSpacer);

            MenuItem miSMS = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "sms" };
            miSMS.Click += this.SMS_MenuItem_Click;
            contextMenu.Items.Add(miSMS);

            MenuItem miEmail = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "email" };
            miEmail.Click += this.Email_MenuItem_Click;
            contextMenu.Items.Add(miEmail);

            MenuItem miSocial = new MenuItem() { Background = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, Foreground = new SolidColorBrush(Colors.White), Header = "social networks" };
            miSocial.Click += this.SocialNetworks_MenuItem_Click;
            contextMenu.Items.Add(miSocial);

            ContextMenuService.SetContextMenu(gParent, contextMenu);
            contextMenu.IsOpen = true;
        }

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }

        private void btnPinSecondary_Click(object sender, EventArgs e)
        {
            if (Config.IsTargetedVersion)
            {
                // Get the new cycleTileData type.
                Type cycleTileDataType = Type.GetType("Microsoft.Phone.Shell.CycleTileData, Microsoft.Phone");

                // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

                // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                var UpdateTileData = cycleTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                Uri smallimage = new Uri("Images/CycleSmall.png", UriKind.Relative);
                Uri mediumimage = new Uri("Images/CycleMedium.png", UriKind.Relative);

                // Set the properties. 
                SetProperty(UpdateTileData, "SmallBackgroundImage", smallimage);

                Uri[] mediumImages = new Uri[1];
                mediumImages[0] = mediumimage;

                SetProperty(UpdateTileData, "CycleImages", mediumImages);

                SetProperty(UpdateTileData, "Title", SelectedCinema.Name);

                ShellTileData tiledata = UpdateTileData as ShellTileData;

                // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                //ShellTile.Create(new Uri(String.Format("/CinemaDetails.xaml?CinemaID={0}&Region={1}&cycle", SelectedCinema.ID, Config.Region.ToString("d")), UriKind.Relative), tiledata);

                // Invoke the new version of ShellTile.Update.
                shellTileType.GetMethod("Create", new Type[] { typeof(Uri), typeof(ShellTileData), typeof(bool) }).Invoke(null, new Object[] { new Uri(String.Format("/CinemaDetails.xaml?CinemaID={0}&Region={1}", SelectedCinema.ID, Config.Region.ToString("d")), UriKind.Relative), tiledata, true });
            }
            else
            {
                StandardTileData standardTileData = new StandardTileData
                {
                    Title = "my cineworld",
                    BackgroundImage = new Uri("Images/AppTile.png", UriKind.Relative),
                    BackContent = SelectedCinema.Name
                };

                // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                ShellTile.Create(new Uri(String.Format("/CinemaDetails.xaml?CinemaID={0}&Region={1}", SelectedCinema.ID, Config.Region.ToString("d")), UriKind.Relative), standardTileData);
            }
        }

        private void btnDrive_Click(object sender, EventArgs e)
        {
            if (SelectedCinema != null)
            {
                BingMapsTask bmt = new BingMapsTask()
                {
                    SearchTerm = String.Format("{0} {1}", SelectedCinema.Address, SelectedCinema.Postcode)
                };
                bmt.Show();
            }
        }

        private void btnViewTrailer_Click(object sender, EventArgs e)
        {
            YouTube.Play(SelectedFilm.YoutubeTrailer);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            YouTube.Play(SelectedFilm.YoutubeTrailer);
        }

        private void btnFavourite_Click(object sender, EventArgs e)
        {
            Config.FavCinemas.Add(SelectedCinema.ID);

            int pos = this.ApplicationBar.Buttons.IndexOf(this.abibAddFav);
            this.ApplicationBar.Buttons.Remove(this.abibAddFav);
            this.ApplicationBar.Buttons.Insert(pos, this.abibRemFav);
        }

        private void btnUnfavourite_Click(object sender, EventArgs e)
        {
            Config.FavCinemas.Remove(SelectedCinema.ID);
            int pos = this.ApplicationBar.Buttons.IndexOf(this.abibRemFav);
            this.ApplicationBar.Buttons.Remove(this.abibRemFav);
            this.ApplicationBar.Buttons.Insert(pos, this.abibAddFav);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (YouTube.CancelPlay())
                e.Cancel = true;

            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                currentContextMenu.IsOpen = false;
                e.Cancel = true;
            }
        }

        private static string GenerateShareMessage(object sender)
        {
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                PerformanceInfo pi = (PerformanceInfo)cm.Tag;

                return String.Format("Shall we go and see \"{0}\" at Cineworld {1} on {2} at {3}? Book here {4}", (string)pi.FilmTitle, SelectedCinema.Name, DateTimeToStringConverter.ConvertData(pi.PerformanceTS), pi.TimeString, (pi.BookUrl).ToString());
            }

            return String.Empty;
        }

        private void SMS_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = GenerateShareMessage(sender);

            if (!String.IsNullOrEmpty(message))
            {
                SmsComposeTask wbt = new SmsComposeTask() { Body = message };
                wbt.Show();
            }
        }

        private void Email_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = GenerateShareMessage(sender);

            if (!String.IsNullOrEmpty(message))
            {
                EmailComposeTask wbt = new EmailComposeTask() { Body = message };
                wbt.Show();
            }
        }

        private void SocialNetworks_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = GenerateShareMessage(sender);

            if (!String.IsNullOrEmpty(message))
            {
                ShareStatusTask wbt = new ShareStatusTask() { Status = message };
                wbt.Show();
            }
        }

        private async void LogBookingStats(object sender, RoutedEventArgs e)
        {
            MenuItem miBuy = (sender as MenuItem);
            miBuy.Click -= this.LogBookingStats;

            try
            {
                MenuItem mi = sender as MenuItem;
                if (mi != null)
                {
                    ContextMenu cm = mi.Parent as ContextMenu;

                    PerformanceInfo pi = (PerformanceInfo)cm.Tag;

                    BookingHistory bh = new BookingHistory(pi);

                    bh.UserId = ExtendedPropertyHelper.GetUserIdentifier();

                    await App.MobileService.GetTable<BookingHistory>().InsertAsync(bh);
                }
            }
            catch { }
        }

        private void BuyTickets_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;

                PerformanceInfo pi = (PerformanceInfo)cm.Tag;

                if (Config.UseMobileWeb)
                {
                    InAppBrowser.NavigationUri = pi.MobileBookingUrl;
                    NavigationService.Navigate(new Uri("/InAppBrowser.xaml", UriKind.Relative));
                }
                else
                {
                    WebBrowserTask wbt = new WebBrowserTask() { Uri = pi.BookUrl };
                    wbt.Show();
                }
            }
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = (sender as Image);

            PersonDetails.castinfo = (CastInfo)img.Tag;

            NavigationService.Navigate(new Uri("/PersonDetails.xaml", UriKind.Relative));
        }
    }
}