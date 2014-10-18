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
using Microsoft.Phone.Tasks;
using System.Threading.Tasks;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Device.Location;
using CineWorld.ViewModels;
using CineWorld.Services;
using CineWorld.Models;
using System.Text;
using Cineworld.Services;

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

        public static CinemaInfo SelectedCinema { get; set; }

        public CinemaDetailsViewModel viewModel = null;

        static Dictionary<int, CinemaDetailsViewModel> vmCache = new Dictionary<int, CinemaDetailsViewModel>();

        public CinemaDetails()
        {
            InitializeComponent();

            if (SelectedCinema != null)
            {
                if (viewModel == null && !vmCache.ContainsKey(SelectedCinema.ID))
                {
                    this.viewModel = new CinemaDetailsViewModel();
                    vmCache.Add(SelectedCinema.ID, viewModel);
                }
                else
                    viewModel = vmCache[SelectedCinema.ID];
            }
            else
            {
                if (viewModel == null)
                    this.viewModel = new CinemaDetailsViewModel();
            }

            this.DataContext = this.viewModel;
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.scWaiting.IsSpinning = bNewVal;
            this.pMain.IsEnabled = !bNewVal;
            this.pMain.Opacity = (bNewVal ? 0 : 1);
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
            this.abibDirections.Click += this.btnDirections_Click;

            this.abibShare = new ApplicationBarIconButton() { IconUri = new Uri("/Images/appbar.share03.png", UriKind.Relative), Text = "share" };
            this.abibShare.Click += this.btnShare_Click;
            
            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("SplashScreenImage-WVGA.jpg", UriKind.Relative)),
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
                    if (!vmCache.ContainsKey(iCin))
                        vmCache[iCin] = viewModel;
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

                    await new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID, false);

                    if (!this.viewModel.Initialised)
                    {
                        this.viewModel.Initialise(SelectedCinema, App.CinemaFilms[iCin]);

                        this.lstShowByDate.IsGroupingEnabled = this.lstCurrent.IsGroupingEnabled = this.lstUpcoming.IsGroupingEnabled = Config.GroupData;

                        if (Config.GroupData)
                        {
                            this.lstShowByDate.SetBinding(LongListSelector.ItemsSourceProperty, new System.Windows.Data.Binding() { Source = this.viewModel.GroupFilmsForDate });
                            ResetLLS();

                            this.lstCurrent.SetBinding(LongListSelector.ItemsSourceProperty, new System.Windows.Data.Binding() { Source = this.viewModel.GroupCurrent });
                            this.lstUpcoming.SetBinding(LongListSelector.ItemsSourceProperty, new System.Windows.Data.Binding() { Source = this.viewModel.GroupUpcoming });
                        }
                        else
                        {
                            this.lstShowByDate.SetBinding(LongListSelector.ItemsSourceProperty, new System.Windows.Data.Binding() { Source = this.viewModel.FilmsForDate });
                            this.lstCurrent.SetBinding(LongListSelector.ItemsSourceProperty, new System.Windows.Data.Binding() { Source = this.viewModel.Current });
                            this.lstUpcoming.SetBinding(LongListSelector.ItemsSourceProperty, new System.Windows.Data.Binding() { Source = this.viewModel.Upcoming });
                        }
                    }
                }
                catch(Exception ex)
                {
                    bError = true;
                }

                if (bError)
                {
                    MessageBox.Show("Error fetching cinema details");
                }

                try
                {
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

            this.SpinAndWait(false);
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

        FilmData filmData = null;

        private void LoadFilmList(List<FilmInfo> films)
        {
            this.calenderFilms.AppointmentSource = this.viewModel.FilmAppointmentSource;
            this.calenderFilms.SelectedValue = DateTime.Today;
        }

        static ContextMenu currentContextMenu = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Grid gParent = btn.Parent as Grid;

            ContextMenu contextMenu = null;

            contextMenu = ContextMenuService.GetContextMenu(gParent);

            if (contextMenu == null)
            {
                contextMenu = new ContextMenu();
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
            }

            contextMenu.IsOpen = true;
        }

        private void btnPinSecondary_Click(object sender, EventArgs e)
        {
            Uri smallimage = new Uri("Images/FlipCycleSmall.png", UriKind.Relative);
            Uri mediumimage = new Uri("Images/FlipCycleMedium.png", UriKind.Relative);

            //Uri[] mediumImages = new Uri[1];
            //mediumImages[0] = mediumimage;

            //CycleTileData NewTileData = new CycleTileData
            //{
            //    SmallBackgroundImage = smallimage,
            //    CycleImages = mediumImages,
            //    Title = SelectedCinema.Name
            //};

            FlipTileData NewTileData = new FlipTileData()
            {
                SmallBackgroundImage = smallimage,
                BackgroundImage = mediumimage,
                Title = SelectedCinema.Name
            };

            // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
            ShellTile.Create(new Uri(String.Format("/CinemaDetails.xaml?CinemaID={0}&Region={1}", SelectedCinema.ID, Config.Region.ToString("d")), UriKind.Relative), NewTileData, false);
        }

        private void btnDirections_Click(object sender, EventArgs e)
        {
            if (SelectedCinema != null)
            {
                SetAppBarDirectionsPrompt();
            }
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

            if (this.gCalendar.Visibility == System.Windows.Visibility.Visible)
            {
                SetCalenderVisibility(false);
                e.Cancel = true;
            }
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FilmInfo fi = (FilmInfo)(sender as Image).Tag;
            
            List<FilmInfo> films = App.CinemaFilms[SelectedCinema.ID];

            ShowPerformances.SelectedFilm = films.First(f => f.EDI == fi.EDI);
            ShowPerformances.SelectedCinema = SelectedCinema;
            ShowPerformances.ShowCinemaDetails = false;
            ShowPerformances.ShowFilmDetails = true;
            NavigationService.Navigate(new Uri("/ShowPerformances.xaml", UriKind.Relative));
        }

        private void ptbDate_ActionIconTapped(object sender, EventArgs e)
        {
            SetCalenderVisibility(true);
        }

        private void ptbDate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Focus();

            SetCalenderVisibility(true);
        }

        private async void RadCalendar_SelectedValueChanged(object sender, Telerik.Windows.Controls.ValueChangedEventArgs<object> e)
        {
            if (!this.viewModel.Initialised)
                return;

            bool hasEntries = this.viewModel.SetFilmsForDate((DateTime)e.NewValue);

            string speakOut = null;

            if (hasEntries)
            {
                SetCalenderVisibility(false);

                this.tbNoFilms.Visibility = System.Windows.Visibility.Collapsed;

                ResetLLS();

                speakOut = String.Format("listsing for {0}", this.viewModel.UserSelectedDate.ToLongDateString());
            }
            else
            {
                this.tbNoFilms.Visibility = System.Windows.Visibility.Visible;

                speakOut = String.Format("no listsing for {0}. Please check tomorrow", this.viewModel.UserSelectedDate.ToLongDateString());
            }

            if (Config.AudioSupport)
            {
                await SpeechSynthesisService.SpeakOutLoud(speakOut);
            }
        }

        private void SetCalenderVisibility(bool CalenderVisible)
        {
            if (CalenderVisible)
            {
                this.pMain.Visibility = System.Windows.Visibility.Collapsed;
                this.ApplicationBar.IsVisible = false;
                this.gCalendar.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.pMain.Visibility = System.Windows.Visibility.Visible;
                this.ApplicationBar.IsVisible = true;
                this.gCalendar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ResetLLS()
        {
            lstShowByDate.UpdateLayout();

            if(Config.GroupData && this.lstShowByDate.ItemsSource != null && this.viewModel.GroupFilmsForDate.Count != 0 && this.lstShowByDate.ItemsSource.Count != 0)
            {
                Group<FilmInfo> selectedgroup = this.viewModel.GroupFilmsForDate.FirstOrDefault(g => g.HasItems);
                if (selectedgroup != null)
                    lstShowByDate.ScrollTo(selectedgroup);
            }
        }

        private void calenderFilms_DisplayDateChanged(object sender, Telerik.Windows.Controls.ValueChangedEventArgs<object> e)
        {
            this.calenderFilms.InvalidateArrange();
            this.calenderFilms.UpdateLayout();
        }

        private async Task SpeakFilmInfoWithShowTimes(FilmInfo fi)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fi.Title);
            sb.AppendFormat("classification {0}\n", fi.Classification);

            sb.Append("Performances at ");
            for (int i = 0; i < fi.Performances.Count; i++)
            {
                sb.Append(fi.Performances[i].TimeString);
                sb.Append(" ");
            }

            await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
        }

        private async Task SpeakFilmInfo(FilmInfo fi)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fi.Title);
            sb.AppendFormat("classification {0}\n", fi.Classification);

            sb.AppendFormat("release date {0}", fi.Release.ToLongDateString());

            await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
        }

        private async void pMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            string speakOut = null;

            switch (this.pMain.SelectedIndex)
            {
                //case 0:
                //    speakOut = String.Format("listsing for {0}", this.viewModel.UserSelectedDate.ToLongDateString());
                //    break;

                case 1:
                case 2:
                    PivotItem pi = this.pMain.SelectedItem as PivotItem;
                    speakOut = String.Format("{0} films", pi.Header);
                    break;

                case 3:
                    speakOut = "cinema details";
                    break;
            }

            await SpeechSynthesisService.SpeakOutLoud(speakOut);
        }

        FilmInfo fi = null;
        private async void FilmListing_Tap(object sender, System.Windows.Input.GestureEventArgs e)
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

            if (!Config.AudioSupport || fi == newFI)
            {
                List<FilmInfo> films = App.CinemaFilms[SelectedCinema.ID];

                ShowPerformances.SelectedFilm = films.First(f => f.EDI == newFI.EDI);
                ShowPerformances.SelectedCinema = SelectedCinema;
                ShowPerformances.ShowCinemaDetails = false;
                ShowPerformances.ShowFilmDetails = true;

                NavigationService.Navigate(new Uri("/ShowPerformances.xaml", UriKind.Relative));
            }
            else
            {
                fi = newFI;

                if (this.pMain.SelectedIndex == 0)
                {
                    await SpeakFilmInfoWithShowTimes(newFI);
                }
                else
                {
                    await SpeakFilmInfo(newFI);
                }
            }
        }
    }
}