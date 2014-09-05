using Callisto.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CinemaDetails : Cineworld.Common.LayoutAwarePage
    {
        internal enum CinemaView
        {
            ShowByDate,
            Current,
            Upcoming,
        }

        CinemaView currentCinemaView = CinemaView.ShowByDate;

        CineworldService cws = new CineworldService();
        CinemaInfo SelectedCinema { get; set; }

        System.Collections.ObjectModel.ObservableCollection<GroupInfoList<object>> FilmsForSelectedDate = new System.Collections.ObjectModel.ObservableCollection<GroupInfoList<object>>();
        //CollectionViewSource cvsViewByDate = new CollectionViewSource() { IsSourceGrouped = true }; 
        
        static SettingsCommand command = null;
        static SettingsCommand command2 = null;
        static SettingsCommand command3 = null;
        static SettingsCommand command4 = null;

        DispatcherTimer dtHideAppBar = new DispatcherTimer();

        public CinemaDetails()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.gBody.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(this.BaseUri, "/Assets/Cineworld_V2_846x468.png")),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            this.AllowSearch(false);

            if (!Landing.bLoaded)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= MainPage_CommandsRequested;

                SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
            } 
            
            DataTransferManager.GetForCurrentView().DataRequested += CinemaDetails_DataRequested;

            SpinAndWait(true);

            int iCin = (int)e.Parameter;

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
                await new MessageDialog("Error fetching cinemas details").ShowAsync();
                //return;
            }
            else
            {
                if (SelectedCinema != null)
                    LoadCinemaDetails();

                try
                {
                    if (!App.CinemaFilms.ContainsKey(iCin))
                    {
                        await new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID, false);
                    }

                    LoadFilmList(App.CinemaFilms[SelectedCinema.ID]);

                    if (!SecondaryTile.Exists(iCin.ToString()))
                    {
                        this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }

                    if (Config.FavCinemas.Contains(iCin))
                    {
                        this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }

                    this.dtHideAppBar.Interval = TimeSpan.FromSeconds(10);
                    this.dtHideAppBar.Tick += dtHideAppBar_Tick;
                    this.cinemaDetailsAppBar.IsOpen = true;
                    this.dtHideAppBar.Start();
                }
                catch
                {
                    bError = true;
                }

                if (bError)
                {
                    await new MessageDialog("Error downloading showtime data").ShowAsync();
                }
            }

            SpinAndWait(false);
        }

        void dtHideAppBar_Tick(object sender, object e)
        {
            this.dtHideAppBar.Tick -= dtHideAppBar_Tick;
            this.dtHideAppBar.Stop();

            this.cinemaDetailsAppBar.IsOpen = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= CinemaDetails_DataRequested;

            base.OnNavigatedFrom(e);
        }

        PerformanceInfo perf = null;

        void CinemaDetails_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (perf != null)
            {
                args.Request.Data.Properties.Title = String.Format("\"{0}\" at Cineworld {1}", perf.FilmTitle, SelectedCinema.Name);
                args.Request.Data.Properties.Description = (String.Format("Shall we go and see it {0} at {1}? Book here {2}", DateTimeToStringConverter.ConvertData(perf.PerformanceTS), perf.PerformanceTS.ToString("HH:mm"), perf.BookUrl.ToString()));
                args.Request.Data.SetUri(perf.BookUrl);

                perf = null;
            }
            else
            {
                args.Request.Data.Properties.Title = "movie tonight?";
                args.Request.Data.SetText(String.Format("Shall we go and see something at Cineworld {0}?", SelectedCinema.Name));
            }
        }

        private void LoadCinemaDetails()
        {
            this.DataContext = SelectedCinema;
        }

        FilmData fdViewViewByDate = null;

        private void LoadFilmList(List<FilmInfo> films)
        {
            this.currentCinemaView = CinemaView.ShowByDate;

            fdViewViewByDate = new FilmData(films);

            cvsShowByDate.Source = FilmsForSelectedDate;
            
            this.SetFilmsForSelectedDate(DateTime.Today);

            //(semanticZoomShowByDate.ZoomedOutView as ListViewBase).ItemsSource = fdViewViewByDate.GetFilmHeaders(dataLetter);

            semanticZoomShowByDate.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoomShowByDate.ViewChangeStarted += semanticZoom_ViewChangeStarted;
        }

        private void SetFilmsForSelectedDate(DateTime userSelection)
        {
            try
            {
                var dataLetter = fdViewViewByDate.GetGroupForDate(userSelection.Date);

                FilmsForSelectedDate.Clear();

                foreach (var entry in dataLetter)
                    FilmsForSelectedDate.Add(entry);

                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).ItemsSource = fdViewViewByDate.GetFilmHeaders(dataLetter);

                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).UpdateLayout();
                
                GroupInfoList<object> group = dataLetter.Find(g => g.Count > 0);

                if (group != null)
                    (this.semanticZoomShowByDate.ZoomedInView as ListViewBase).ScrollIntoView(group);
            }
            catch { }
        }

        private void SetCinemaView(CinemaView newCinemaView)
        {
            if (this.currentCinemaView == newCinemaView)
                return;

            this.currentCinemaView = newCinemaView;

            if(currentCinemaView == CinemaView.ShowByDate)
            {
                this.SetFilmsForSelectedDate((DateTime)this.dpShowing.Value);
                return;
            }

            List<GroupInfoList<object>> filmDataSet = null;

            switch (this.currentCinemaView)
            {
                case CinemaView.Current:
                    filmDataSet = fdViewViewByDate.CurrentFilmGroups;
                    break;

                case CinemaView.Upcoming:
                    filmDataSet = fdViewViewByDate.UpcomingFilmGroups;
                    break;
            }

            if (filmDataSet != null)
            {
                FilmsForSelectedDate.Clear();

                foreach (var entry in filmDataSet)
                    FilmsForSelectedDate.Add(entry);

                
                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).ItemsSource = fdViewViewByDate.GetFilmHeaders(filmDataSet);

                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).UpdateLayout();

                GroupInfoList<object> group = filmDataSet.Find(g => g.Count > 0);

                if (group != null)
                    (this.semanticZoomShowByDate.ZoomedInView as ListViewBase).ScrollIntoView(group);
            }
        }


        void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(HeaderItem))
            {
                HeaderItem hi = (HeaderItem)e.SourceItem.Item;

                if (hi.Group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = hi.Group };
            }
        }

        private void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            if (command == null)
            {
                command = new SettingsCommand("Support", "Support", (x) =>
                {
                    SettingsFlyout settings = new SettingsFlyout();
                    settings.Content = new About();
                    settings.HeaderBrush = new SolidColorBrush(new HexColour("#FFB51C10"));
                    settings.Background = new SolidColorBrush(Colors.White);
                    settings.HeaderText =
                        "Support";
                    settings.IsOpen = true;
                });
            }

            if (!args.Request.ApplicationCommands.Contains(command))
                args.Request.ApplicationCommands.Add(command);

            if (command2 == null)
            {
                command2 = new SettingsCommand("PrivacyPolicy", "Privacy Policy", (x) =>
                {
                    SettingsFlyout settings = new SettingsFlyout();
                    settings.Content = new PrivacyPolicy();
                    settings.HeaderBrush = new SolidColorBrush(new HexColour("#FFB51C10"));
                    settings.Background = new SolidColorBrush(Colors.White);
                    settings.HeaderText =
                        "Privacy Policy";
                    settings.IsOpen = true;
                });
            }

            if (!args.Request.ApplicationCommands.Contains(command2))
                args.Request.ApplicationCommands.Add(command2);

            if (command3 == null)
            {
                command3 = new SettingsCommand("Settings", "Options", (x) =>
                {
                    SettingsFlyout settings = new SettingsFlyout();
                    settings.Content = new Settings();
                    settings.HeaderBrush = new SolidColorBrush(new HexColour("#FFB51C10"));
                    settings.Background = new SolidColorBrush(Colors.White);
                    settings.HeaderText =
                        "Options";
                    settings.IsOpen = true;
                });
            }

            if (!args.Request.ApplicationCommands.Contains(command3))
                args.Request.ApplicationCommands.Add(command3);

        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SemanticZoom sz = this.semanticZoomShowByDate;

            sz.IsZoomedInViewActive = false;
        }

        private void btnFilledViewOnly_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
        }

        private void radShowingToday_Click(object sender, RoutedEventArgs e)
        {
            //if (radShowingToday.IsChecked == true)
            //{
            //    this.semanticZoomViewToday.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //    this.semanticZoomViewByDate.Visibility = this.semanticZoomViewAll.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //}
            //else if (radViewByDate.IsChecked == true)
            //{
            //    this.semanticZoomViewByDate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //    this.semanticZoomViewToday.Visibility = this.semanticZoomViewAll.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //}
            //else
            //{
            //    this.semanticZoomViewAll.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //    this.semanticZoomViewByDate.Visibility = this.semanticZoomViewToday.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //}
        }

        private async void btnPinToStartMenu_Click(object sender, RoutedEventArgs e)
        {
            SecondaryTile secondary = new SecondaryTile(SelectedCinema.ID.ToString(),
                SelectedCinema.Name, SelectedCinema.Name, Config.Region.ToString(),
                TileOptions.ShowNameOnLogo, new Uri("ms-appx:///Assets/Logo.png"));

            if (await secondary.RequestCreateAsync())
            {
                this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }


        private async void btnUnPinToStartMenu_Click(object sender, RoutedEventArgs e)
        {
            string cinemastr = SelectedCinema.ID.ToString();
            IReadOnlyList<SecondaryTile> tiles = await SecondaryTile.FindAllAsync();
            SecondaryTile tile = tiles.FirstOrDefault(t => t.TileId == cinemastr);

            if (tile != null && await tile.RequestDeleteAsync())
            {
                this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            }
        }

        private void btnUnfavourite_Click(object sender, RoutedEventArgs e)
        {
            List<int> Cinemas = Config.FavCinemas;

            if (!Cinemas.Contains(SelectedCinema.ID))
            {
                Cinemas.Remove(SelectedCinema.ID);
                Config.FavCinemas = Cinemas;
            }

            this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void btnFavourite_Click(object sender, RoutedEventArgs e)
        {
            List<int> Cinemas = Config.FavCinemas;

            if (!Cinemas.Contains(SelectedCinema.ID))
            {
                Cinemas.Add(SelectedCinema.ID);
                Config.FavCinemas = Cinemas;
            }

            this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void btnViewOnMap_Click(object sender, RoutedEventArgs e)
        {
            string BingMapsUri = String.Format("http://www.bing.com/maps/default.aspx?cp={0}~{1}&lvl=18&style=u", SelectedCinema.Latitude, SelectedCinema.Longitute);
            Uri uri = new Uri(BingMapsUri, UriKind.Absolute);
            await Launcher.LaunchUriAsync(uri);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            PerformanceInfo pi = (PerformanceInfo)(sender as Button).CommandParameter;
            
            PopupMenu menu = new PopupMenu();
            UICommand cmdBuyTickets = new UICommand("buy tickets", async (command) =>
            {
                if (Config.UseMobileWeb)
                {
                    InAppBrowser.NavigationUri = pi.BookUrl;
                    this.Frame.Navigate(typeof(InAppBrowser));
                }
                else
                {
                    await Launcher.LaunchUriAsync(pi.BookUrl);
                }
            });
            menu.Commands.Add(cmdBuyTickets);

            UICommand cmdShare = new UICommand("share details", (command) =>
            {
                perf = pi;
                DataTransferManager.ShowShareUI();
            });

            menu.Commands.Add(cmdShare);

            var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender)); 
            if (chosenCommand == null) // The command is null if no command was invoked. 
            {
                perf = null;
            } 
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        } 

        private void btnReviews_Click(object sender, RoutedEventArgs e)
        {
            Flyout flyOut = new Flyout();
            flyOut.Content = new ViewReviews();
            ViewReviews.ReviewTarget = Review.ReviewTargetDef.Cinema;
            ViewReviews.SelectedCinema = SelectedCinema;
            flyOut.Background = new SolidColorBrush(Colors.White);

            flyOut.PlacementTarget = sender as UIElement;
            flyOut.Placement = PlacementMode.Top;
            flyOut.IsOpen = true;
        }

        private void btnRateCinema_Click(object sender, RoutedEventArgs e)
        {
            Flyout flyOut = new Flyout();
            flyOut.Content = new Review();
            Review.ReviewTarget = Review.ReviewTargetDef.Cinema;
            Review.SelectedCinema = SelectedCinema;

            flyOut.Background = new SolidColorBrush(Colors.White);

            flyOut.PlacementTarget = sender as UIElement;
            flyOut.Placement = PlacementMode.Top;
            flyOut.IsOpen = true;
        }

        private void radShowByDate_Click(object sender, RoutedEventArgs e)
        {
            this.SetCinemaView(CinemaView.ShowByDate);
        }

        private void radCurrent_Click(object sender, RoutedEventArgs e)
        {
            this.SetCinemaView(CinemaView.Current);
        }

        private void radUpcoming_Click(object sender, RoutedEventArgs e)
        {
            this.SetCinemaView(CinemaView.Upcoming);
        }

        private void dpShowing_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.SetFilmsForSelectedDate((DateTime)e.NewValue);
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilmInfo fi = (FilmInfo)(sender as Image).Tag;

            List<FilmInfo> films = App.CinemaFilms[SelectedCinema.ID];

            ShowPerformances.SelectedFilm = films.Find(f => f.EDI == fi.EDI);

            ShowPerformances.SelectedCinema = SelectedCinema;
            ShowPerformances.ShowFilmDetails = true;
            ShowPerformances.ShowCinemaDetails = false;

            this.Frame.Navigate(typeof(ShowPerformances));
        }
    }
        
}
