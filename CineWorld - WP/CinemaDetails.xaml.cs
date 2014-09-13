using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Cineworld;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;

namespace CineWorld
{
    public partial class CinemaDetails : PhoneApplicationPage
    {
        ApplicationBarIconButton abibPin = null;
        ApplicationBarIconButton abibAddFav = null;
        ApplicationBarIconButton abibRemFav = null;
        ApplicationBarIconButton abibDirections = null;
        ApplicationBarIconButton abibShare = null;

        static bool firsttime = true;
        bool bLoaded = false;

        System.Collections.ObjectModel.ObservableCollection<Group<FilmInfo>> FilmsForSelectedDate = new System.Collections.ObjectModel.ObservableCollection<Group<FilmInfo>>();

        //static FeedbackOverlay feedback;
        
        public static CinemaInfo SelectedCinema { get; set; }

        public CinemaDetails()
        {
            InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.scWaiting.IsSpinning = bNewVal;
            this.pMain.IsEnabled = !bNewVal;
            this.pMain.Opacity = (bNewVal ? 0.5 : 1);
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SpinAndWait(true);

            this.abibAddFav = new ApplicationBarIconButton() { Text = "favourite", IconUri = new Uri("/Images/appbar.favs.addto.rest.png", UriKind.Relative) };
            this.abibAddFav.Click += this.btnFavourite_Click;

            this.abibRemFav = new ApplicationBarIconButton() { Text = "unfavourite", IconUri = new Uri("/Images/appbar.favs.rem.rest.png", UriKind.Relative) };
            this.abibRemFav.Click += this.btnUnfavourite_Click;

            this.abibPin = new ApplicationBarIconButton() { Text = "pin", IconUri = new Uri("/Images/appbar.pin.png", UriKind.Relative) };
            this.abibPin.Click += this.btnPinSecondary_Click;

            this.abibDirections = new ApplicationBarIconButton() { Text = "drive", IconUri = new Uri("/Images/route.png", UriKind.Relative) };
            this.abibDirections.Click += this.btnDrive_Click;

            this.abibShare = new ApplicationBarIconButton() { IconUri = new Uri("/Images/appbar.share03.png", UriKind.Relative), Text = "share" };
            this.abibShare.Click += this.btnShare_Click;

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("SplashScreenImage.jpg", UriKind.Relative)),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            if (!bLoaded)
            {
                int iCin = int.MinValue;

                if (this.NavigationContext.QueryString.ContainsKey("CinemaID") && SelectedCinema == null)
                {
                    iCin = int.Parse(this.NavigationContext.QueryString["CinemaID"]);
                }
                else
                {
                    iCin = SelectedCinema.ID;
                }

                if (this.NavigationContext.QueryString.ContainsKey("Region"))
                    Config.Region = (Config.RegionDef)int.Parse(this.NavigationContext.QueryString["Region"]);

                bool bError = false;
                try
                {
                    if (App.Cinemas == null || App.Cinemas.Count == 0)
                    {
                        LocalStorageHelper lsh = new LocalStorageHelper();
                        await lsh.DownloadFiles(false);

                        await lsh.DeserialiseObjects();
                    }

                    SelectedCinema = App.Cinemas[iCin];
                }
                catch
                {
                    bError = true;
                }

                if (bError)
                {
                    MessageBox.Show("Error fetching cinemas details");
                }

                if (SelectedCinema != null)
                {
                    LoadCinemaDetails();
                }

                try
                {
                    await new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID, false);

                    LoadFilmList(App.CinemaFilms[iCin]);
                }
                catch
                {
                    bError = true;
                }

                if (bError)
                {
                    MessageBox.Show("Error downloading showtime data");
                }

                bLoaded = true;
            }

            this.ApplicationBar.Buttons.Clear();
            this.ApplicationBar.Buttons.Add(this.abibDirections);

            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(String.Format("CinemaID={0}", SelectedCinema.ID)));
            if (TileToFind == null)
                this.ApplicationBar.Buttons.Add(this.abibPin);

            if (Config.FavCinemas.Contains(SelectedCinema.ID))
                this.ApplicationBar.Buttons.Add(this.abibRemFav);
            else
                this.ApplicationBar.Buttons.Add(this.abibAddFav);

            if (!this.ApplicationBar.Buttons.Contains(abibShare))
                this.ApplicationBar.Buttons.Add(abibShare);

            SpinAndWait(false);
        }

        void btnShare_Click(object sender, EventArgs e)
        {
            SetAppBarSharePrompt();
        }

        private void SetAppBarSharePrompt()
        {
            string message = String.Format("Shall we go and see something at Cineworld {0}?", SelectedCinema.Name);

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

        private void LoadCinemaDetails()
        {
            this.DataContext = SelectedCinema;
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
            Review.SelectedCinema = CinemaDetails.SelectedCinema;
            Review.ReviewTarget = Review.ReviewTargetDef.Cinema;

            NavigationService.Navigate(new Uri("/Review.xaml", UriKind.Relative));
        }

        FilmData cd = null;

        private void LoadFilmList(List<FilmInfo> films)
        {
            cd = new FilmData(films);

            this.dpShowing.Value = DateTime.Today;
            this.lstShowByDate.ItemsSource = FilmsForSelectedDate;
            
            this.SetFilmsForSelectedDate(DateTime.Today);

            List<FilmInfo> current = new List<FilmInfo>();
            List<FilmInfo> upcomig = new List<FilmInfo>();

            foreach (var film in films)
            {
                if (film.Release <= DateTime.UtcNow)
                    current.Add(film);
                else
                    upcomig.Add(film);
            }

            FilmData cdCurrent = new FilmData(current);

            var dataLetterCurrent = cdCurrent.GetGroupsByLetter();

            this.lstCurrent.ItemsSource = dataLetterCurrent;

            FilmData cdUpcoming = new FilmData(upcomig);

            var dataLetterUpcoming = cdUpcoming.GetGroupsByLetter();

            this.lstUpcoming.ItemsSource = dataLetterUpcoming;
        }

        private void dpShowing_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            DateTime dt = e.NewDateTime != null ? e.NewDateTime.Value : DateTime.MinValue;
            SetFilmsForSelectedDate(dt);
        }

        private void SetFilmsForSelectedDate(DateTime userSelection)
        {
            try
            {
                var dataLetter = cd.GetGroupForDate(userSelection);

                FilmsForSelectedDate.Clear();

                foreach (var entry in dataLetter)
                    FilmsForSelectedDate.Add(entry);

                lstShowByDate.UpdateLayout();

                if (FilmsForSelectedDate.Count != 0)
                {
                    Group<FilmInfo> selectedgroup = FilmsForSelectedDate.FirstOrDefault(g => g.HasItems);
                    if (selectedgroup != null)
                        lstShowByDate.ScrollTo(selectedgroup);
                }

                this.tbNoFilmsForDate.Visibility = FilmsForSelectedDate.Count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            catch { }
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
        
        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (currentContextMenu != null && currentContextMenu.IsOpen)
            {
                currentContextMenu.IsOpen = false;
                e.Cancel = true;
            }
        }

        private void FilmListing_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FilmInfo newFI = null;

            if (sender is Image)
            {
                Image img = sender as Image;

                newFI = img.Tag as FilmInfo;
            }
            else if (sender is Grid)
            {
                Grid grid = sender as Grid;

                newFI = grid.Tag as FilmInfo;
            }
            else
                return;

            if(newFI == null)
                return;

            List<FilmInfo> films = App.CinemaFilms[SelectedCinema.ID];

            ShowPerformances.SelectedFilm = films.First(f => f.EDI == newFI.EDI);
            ShowPerformances.SelectedCinema = SelectedCinema;
            ShowPerformances.ShowCinemaDetails = false;
            ShowPerformances.ShowFilmDetails = true;
            NavigationService.Navigate(new Uri("/ShowPerformances.xaml", UriKind.Relative));
        }

        private void btnShowByDate_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}