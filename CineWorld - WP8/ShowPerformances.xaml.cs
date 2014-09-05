    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Cineworld;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using MyToolkit.Multimedia;
using System.Windows.Media;
using Coding4Fun.Toolkit.Controls;
using Nokia.Music.Tasks;
using System.Text;
using Cineworld.Services;

namespace CineWorld
{
    public partial class ShowPerformances : PhoneApplicationPage
    {
        //static Dates SelectedDates { get; set; }
        public static CinemaInfo SelectedCinema { get; set; }
        public static FilmInfo SelectedFilm { get; set; }

        public static bool ShowFilmDetails { get; set; }
        public static bool ShowCinemaDetails { get; set; }

        ApplicationBarIconButton abibPin = null;
        ApplicationBarIconButton abibAddFav = null;
        ApplicationBarIconButton abibRemFav = null;
        ApplicationBarIconButton abibDirections = null;

        ApplicationBarIconButton abibTrailer = null;
        ApplicationBarIconButton abibShare = null;
        ApplicationBarIconButton abibSoundTrack = null;

        bool bLoaded = false;

        public ShowPerformances()
        {
            InitializeComponent();
        }

        //private void SpinAndWait(bool bNewVal)
        //{
        //    this.scWaiting.IsSpinning = bNewVal;
        //    this.pMain.IsEnabled = !bNewVal;
        //    this.pMain.Opacity = (bNewVal ? 0.5 : 1);
        //}

        

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.abibAddFav = new ApplicationBarIconButton() { Text = "favourite", IconUri = new Uri("/Images/appbar.favs.addto.rest.png", UriKind.Relative) };
            this.abibAddFav.Click += this.btnFavourite_Click;

            this.abibRemFav = new ApplicationBarIconButton() { Text = "unfavourite", IconUri = new Uri("/Images/appbar.favs.rem.rest.png", UriKind.Relative) };
            this.abibRemFav.Click += this.btnUnfavourite_Click;

            this.abibPin = new ApplicationBarIconButton() { Text = "pin", IconUri = new Uri("/Images/appbar.pin.png", UriKind.Relative) };
            this.abibPin.Click += this.btnPinSecondary_Click;

            this.abibDirections = new ApplicationBarIconButton() { Text = "drive", IconUri = new Uri("/Images/route.png", UriKind.Relative) };
            this.abibDirections.Click += this.btnDrive_Click;

            this.abibTrailer = new ApplicationBarIconButton() { Text = "trailer", IconUri = new Uri("/Images/appbar.transport.play.rest.png", UriKind.Relative) };
            this.abibTrailer.Click += this.btnViewTrailer_Click;

            this.abibShare = new ApplicationBarIconButton() { Text = "share", IconUri = new Uri("/Images/appbar.share03.png", UriKind.Relative) };
            this.abibShare.Click += this.btnShare_Click;

            this.abibSoundTrack = new ApplicationBarIconButton() { IconUri = new Uri("/Images/appbar.notes.rest.png", UriKind.Relative), Text = "sounds track" };
            this.abibSoundTrack.Click += this.abibSoundTrack_Click;

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

            bool bError = false;
            try
            {   
                this.DataContext = this;

                if (ShowCinemaDetails)
                    await this.LoadCinemaDetails();
                
                if (ShowFilmDetails)
                    this.LoadFilmDetails();

                var query = from item in SelectedFilm.Performances
                            orderby ((PerformanceInfo)item).PerformanceTS
                            group item by ((PerformanceInfo)item).PerformanceTS.Date into g
                            select new Group<PerformanceInfo>(DateTimeToStringConverter.ConvertData(g.Key.Date), g);

                List<Group<PerformanceInfo>> shows = new List<Group<PerformanceInfo>>(query);
                this.lbPerformances.ItemsSource = shows;
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
            if (pMain.Items.Contains(piCinema))
                this.pMain.Items.Remove(piCinema);
                        
            if (!String.IsNullOrWhiteSpace(SelectedFilm.YoutubeTrailer))
            {
                this.ApplicationBar.Buttons.Add(this.abibTrailer);
            }

            if (SelectedFilm.FilmCast == null || SelectedFilm.FilmCast.Count == 0)
            {
                this.pMain.Items.Remove(this.piCast);
            }

            this.ApplicationBar.Buttons.Add(this.abibSoundTrack);

            this.ApplicationBar.Buttons.Add(this.abibShare);
        }

        private void SetMusicSearchMenu()
        {
            string searchterm = String.Format("{0} soundtrack", SelectedFilm.CleanTitle);

            new AppBarPrompt(
                new AppBarPromptAction("XBox Music", () =>
                {
                    new MarketplaceSearchTask()
                    {
                        ContentType = MarketplaceContentType.Music,
                        SearchTerms = searchterm
                    }.Show();

                }),
                new AppBarPromptAction("Nokia Music", () =>
                {
                    new MusicSearchTask()
                    {
                        SearchTerms = searchterm
                    }.Show();
                })
                ) { Foreground = new SolidColorBrush(Colors.White) }.Show();
        }

        void abibSoundTrack_Click(object sender, EventArgs e)
        {
            if (Config.AllowNokiaMusicSearch == true)
                SetMusicSearchMenu();
            else
            {
                string searchterm = String.Format("{0} soundtrack", SelectedFilm.CleanTitle);

                new MarketplaceSearchTask()
                {
                    ContentType = MarketplaceContentType.Music,
                    SearchTerms = searchterm
                }.Show();
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
            Task taskCinemaFilmListing = null;

            if (SelectedFilm.Performances == null || SelectedFilm.Performances.Count == 0)
            {
                taskCinemaFilmListing = new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID);
            }

            if (pMain.Items.Contains(piFilmDetails))
                this.pMain.Items.Remove(piFilmDetails);

            if (pMain.Items.Contains(piCast))
                this.pMain.Items.Remove(piCast);

            if (pMain.Items.Contains(piReviews))
                this.pMain.Items.Remove(piReviews);

            if (!pMain.Items.Contains(piCinema))
                pMain.Items.Add(piCinema);

            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(String.Format("CinemaID={0}", SelectedCinema.ID)));

            this.ApplicationBar.Buttons.Add(this.abibDirections);

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

        private void btnPinSecondary_Click(object sender, EventArgs e)
        {
            Uri smallimage = new Uri("Images/CycleSmall.png", UriKind.Relative);
            Uri mediumimage = new Uri("Images/CycleMedium.png", UriKind.Relative);

            Uri[] mediumImages = new Uri[1];
            mediumImages[0] = mediumimage;

            CycleTileData NewTileData = new CycleTileData
            {
                SmallBackgroundImage = smallimage,
                CycleImages = mediumImages,
                Title = SelectedCinema.Name
            };

            // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
            ShellTile.Create(new Uri(String.Format("/CinemaDetails.xaml?CinemaID={0}&Region={1}", SelectedCinema.ID, Config.Region.ToString("d")), UriKind.Relative), NewTileData, true);
        }

        async Task RequestDrivingDirections()
        {
            // Assemble the Uri to launch.
            Uri uri = new Uri(String.Format("ms-drive-to:?destination.latitude={0}&destination.longitude={1}&destination.name=Cineworld", SelectedCinema.Latitude, SelectedCinema.Longitute));

            // Launch the Uri.
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (!success)
            {
                MessageBox.Show("error loading driving directions");
            }
        }

        async Task RequestWalkingDirections()
        {
            // Assemble the Uri to launch.
            Uri uri = new Uri(String.Format("ms-walk-to:?destination.latitude={0}&destination.longitude={1}&destination.name=Cineworld", SelectedCinema.Latitude, SelectedCinema.Longitute));

            // Launch the Uri.
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (!success)
            {
                MessageBox.Show("error loading walking directions");
            }
        }

        private void SetAppBarDirectionsPrompt()
        {
            new AppBarPrompt(
                new AppBarPromptAction("drive", async () =>
                {
                    await RequestDrivingDirections();
                }),
                new AppBarPromptAction("walk", async () =>
                {
                    await RequestWalkingDirections();
                })
                ) { Foreground = new SolidColorBrush(Colors.White) }.Show();
        }

        private void btnDrive_Click(object sender, EventArgs e)
        {
            if (SelectedCinema != null)
            {
                this.SetAppBarDirectionsPrompt();
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            YouTube.Play(SelectedFilm.YoutubeTrailer);
        }

        private void btnViewTrailer_Click(object sender, EventArgs e)
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

        private async void pMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            string speakOut = null;

            if (this.pMain.SelectedItem == this.piCast)
            {
                speakOut = "film cast";
            }
            else if (this.pMain.SelectedItem == this.piCinema)
            {
                speakOut = "cinema details";
            }
            else if (this.pMain.SelectedItem == this.piFilmDetails)
            {
                speakOut = String.Format("overview of {0}", SelectedFilm.Title);
            }
            else if (this.pMain.SelectedItem == this.piReviews)
            {
                speakOut = "user reviews";
            }
            else
            {
                speakOut = "film performances";
            }

            await SpeechSynthesisService.SpeakOutLoud(speakOut);
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

        private async Task SpeakCastInfo(CastInfo ci)
        {
            await SpeechSynthesisService.SpeakOutLoud(ci.Title);
        }

        private async void FilmPoster_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            await SpeakFilmInfo(SelectedFilm);
        }

        private async Task SpeakFilmInfo(FilmInfo fi)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fi.Title);
            sb.AppendFormat("classification {0}\n", fi.Classification);

            if (fi.VoteCount > 0)
                sb.AppendFormat("average rating {0}, {1} reviews\n", fi.AverageRating.ToString("N2"), fi.VoteCount);

            sb.AppendFormat("release date {0}\n", fi.Release.ToLongDateString());

            if (fi.Runtime > 0)
                sb.AppendFormat("duration {0} minutes\n", fi.Runtime);

            sb.AppendFormat("overview {0}", fi.Overview);

            await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
        }

        CastInfo ci = null;

        private async void Cast_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid g = (sender as Grid);

            CastInfo newCI = (CastInfo)g.Tag;

            if (Config.AudioSupport && ci != newCI)
            {
                ci = newCI;

                await SpeakCastInfo(newCI);

                return;
            }
            else
            {
                PersonDetails.castinfo = newCI;

                NavigationService.Navigate(new Uri("/PersonDetails.xaml", UriKind.Relative));
            }
        }

        private async void ListForDay_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            StackPanel sp = (sender as StackPanel);
            Group<PerformanceInfo> performances = sp.Tag as Group<PerformanceInfo>;

            if (performances != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Performance for {0}\n", performances.GroupTitle);

                foreach (var entry in performances)
                {
                    sb.Append(entry.TimeString);
                    sb.AppendFormat(" ");
                }

                await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
            }
        }

        private async void CinemaDetails_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(SelectedCinema.Name);
            sb.AppendLine(SelectedCinema.FullAddress);

            await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
        }
    }
}