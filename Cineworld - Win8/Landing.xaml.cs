using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.ApplicationSettings;
using Callisto.Controls;
using Windows.Devices.Geolocation;
using Coding4Fun.Toolkit.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Landing : Cineworld.Common.LayoutAwarePage
    {
        public static bool bLoaded = false;
        HashSet<int> PinnedCinemas = new HashSet<int>();

        static SettingsCommand command = null;
        static SettingsCommand command2 = null;
        static SettingsCommand command3 = null;
        static SettingsCommand command4 = null;

        DispatcherTimer dtHideAppBar = new DispatcherTimer();


        public Landing()
        {
            this.InitializeComponent();
        }

        public static void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
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

            if(!args.Request.ApplicationCommands.Contains(command))
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

            if(!args.Request.ApplicationCommands.Contains(command2))
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

            if(!args.Request.ApplicationCommands.Contains(command3))
                args.Request.ApplicationCommands.Add(command3);

            
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
            this.wpHubTiles.Opacity = bNewVal ? 0.5 : 1;
            this.wpHubTiles.IsHitTestVisible = !bNewVal;

            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
                        
            this.SpinAndWait(true);

            if(!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(this.BaseUri, "/Assets/Cineworld_V2_846x468.png")),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            Config.RegionChanged -= Config_RegionChanged;
            Config.RegionChanged += Config_RegionChanged;

            await ExecuteInitialDataLoad();

            this.SpinAndWait(false);

            
            SettingsPane.GetForCurrentView().CommandsRequested -= MainPage_CommandsRequested;

            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;

            bLoaded = true;
            
            DataTransferManager.GetForCurrentView().DataRequested += Default_DataRequested;

            if (Config.UseLocation)
            {
                await LoadNearestCinema();
            }

            this.dtHideAppBar.Interval = TimeSpan.FromSeconds(10);
            this.dtHideAppBar.Tick += dtHideAppBar_Tick;
            this.appBar.IsOpen = true;
            this.dtHideAppBar.Start();
        }

        void dtHideAppBar_Tick(object sender, object e)
        {
            this.dtHideAppBar.Tick -= dtHideAppBar_Tick;
            this.dtHideAppBar.Stop();

            this.appBar.IsOpen = false;
        }


        private async Task ExecuteInitialDataLoad(bool bForce = false)
        {
            SpinAndWait(true);
            bool bErrored = false;

            this.PinnedCinemas.Clear();

            if (!bLoaded)
            {
                try
                {
                    await Initialise(bForce);
                }
                catch (Exception ex)
                {
                    bErrored = true;
                }

                if (bErrored)
                {
                    await (new MessageDialog("Error fetching cinema / film listing").ShowAsync());
                    this.BottomAppBar.IsSticky = true;
                    this.BottomAppBar.IsOpen = true;
                }
                else
                {
                    this.BottomAppBar.IsSticky = false;
                    this.BottomAppBar.IsOpen = false;
                }

                bLoaded = true;
            }

            if (!bErrored)
            {
                await InstantiateTiles(bForce);

                this.SpinAndWait(false);
            }
        }

        private async Task InstantiateTiles(bool bForce = false)
        {
            this.wpHubTiles.Children.Clear();

            await this.LoadFilmData();

            this.SetTile(-1);

            await this.LoadPinnedAndFavouriteCinemas();
        }

        private void SetTile(int iCin, CinemaInfo ci = null, string additionalTextualInfo = null)
        {
            Tile t = new Tile()
            {
                CommandParameter = iCin,
                Margin = new Thickness(20, 20, 0, 0),
                Width = 300,
                Height = 300,
                Foreground = new SolidColorBrush(Colors.White),
                Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Background.png")),
                    Stretch = Stretch.UniformToFill
                }
            };

            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            if (ci != null && additionalTextualInfo != null)
            {
                TextBlock tbTopText = new TextBlock() { Text = additionalTextualInfo, Margin = new Thickness(12), VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top};
                Grid.SetColumn(tbTopText, 0);
                Grid.SetRow(tbTopText, 0);
                g.Children.Add(tbTopText);
            }

            TextBlock tbBottomText = new TextBlock() { Margin = new Thickness(12), VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom, TextWrapping = TextWrapping.Wrap };
            if (ci == null)
                tbBottomText.Text = "All Cinemas";
            else
                tbBottomText.Text = ci.Name;
            
            t.Click += Tile_Click;

            Grid.SetColumn(tbBottomText, 0);
            Grid.SetRow(tbBottomText, 1);

            g.Children.Add(tbBottomText);

            t.Content = g;

            this.wpHubTiles.Children.Add(t);
        }

        private static async Task Initialise(bool bForce = false)
        {
            LocalStorageHelper lsh = new LocalStorageHelper();
            await lsh.DownloadFiles(bForce);

            await lsh.DeserialiseObjects();
        }

        async void Config_RegionChanged()
        {
            bLoaded = false;

            if (this.Frame.CanGoBack)
                this.GoHome(this, new RoutedEventArgs());
            else
            {
                await ExecuteInitialDataLoad(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            DataTransferManager.GetForCurrentView().DataRequested -= Default_DataRequested;
        }

        void Default_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = "Shall we watch a movie ?";

            args.Request.Data.SetText("Lets watch a movie... what shall we watch and where ? I've cineworld app open and waiting for your reply!");
        }
        
        private async Task LoadPinnedAndFavouriteCinemas(bool bForce = false)
        {
            LocalStorageHelper lsh = new LocalStorageHelper();

            List<Task> cinemaDownloads = new List<Task>();
            foreach (var tile in await SecondaryTile.FindAllAsync())
            {
                int iCin = int.Parse(tile.TileId);
                PinnedCinemas.Add(iCin);
                cinemaDownloads.Add(lsh.GetCinemaFilmListings(iCin, bForce));

                var SelectedCinema = App.Cinemas[iCin];

                if (SelectedCinema != null)
                    this.SetTile(iCin, SelectedCinema, "Pinned");
            }

            if (Config.FavCinemas != null)
            {
                foreach (int iCin in Config.FavCinemas)
                {
                    if (PinnedCinemas.Contains(iCin))
                        continue;

                    if (App.Cinemas.ContainsKey(iCin))
                    {
                        PinnedCinemas.Add(iCin);
                        cinemaDownloads.Add(lsh.GetCinemaFilmListings(iCin, bForce));

                        CinemaInfo ci = App.Cinemas[iCin];
                        string message = null;
                        try
                        {
                            this.SetTile(iCin, ci, "Favourite");
                        }
                        catch (Exception ex)
                        {
                            message = ex.Message;
                        }
                    }
                }
            }

            if (cinemaDownloads.Count > 0)
                await Task.WhenAll(cinemaDownloads);
        }

        private async Task LoadNearestCinema()
        {
            Geolocator locator = new Geolocator();
            
            Exception e = null;
            try
            {
                Geoposition pos = await locator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(20));
                
                IEnumerable<CinemaInfo> oc = App.Cinemas.Values.OrderBy(c => GeoMath.Distance(pos.Coordinate.Latitude, pos.Coordinate.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(1);
                if (oc.Count() > 0)
                {
                    int iCin = oc.First().ID;

                    if (!PinnedCinemas.Contains(iCin))
                    {
                        if (App.Cinemas.ContainsKey(iCin))
                        {
                            PinnedCinemas.Add(iCin);
                            
                            CinemaInfo ci = App.Cinemas[iCin];
                            string message = null;
                            try
                            {
                                this.SetTile(iCin, ci, "Nearest");
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                e = ex;
            }
        }

        private async Task LoadFilmData()
        {
            BaseStorageHelper bsh = new BaseStorageHelper();
            List<Uri> posterUrls = await bsh.GetImageList();

            ImageTile it = new ImageTile() { Width = 300, Height = 300, Rows = 2, Columns = 3, Margin = new Thickness(20, 20, 0, 0), AnimationType = ImageTileAnimationTypes.Fade, ItemsSource = posterUrls };
            Grid g = new Grid()
            {
                Width = 280,
                Height = 280,
                Margin = new Thickness(10)
            };
            TextBlock tb = new TextBlock() { Text = "All Films", VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom };
            
            Grid.SetRow(tb, 0);
            Grid.SetColumn(tb, 0);
            g.Children.Add(tb);

            it.Content = g;

            it.Click += ImageTile_Click;
            this.wpHubTiles.Children.Add(it);
        }

        void ImageTile_Click(object sender, RoutedEventArgs e)
        {
            (sender as ImageTile).IsFrozen = true;
            this.Frame.Navigate(typeof(ListFilms));
        }

        void Tile_Click(object sender, RoutedEventArgs e)
        {
            Tile t = sender as Tile;
            if (t.CommandParameter != null && ((int)t.CommandParameter) != -1)
            {
                this.Frame.Navigate(typeof(CinemaDetails), t.CommandParameter);
            }
            else
                this.Frame.Navigate(typeof(ListCinemas));
        }

        private void btnFilledViewOnly_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            bLoaded = false;
            await ExecuteInitialDataLoad(true);
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout settings = new SettingsFlyout();
            settings.Content = new About();
            settings.HeaderBrush = new SolidColorBrush(new HexColour("#FFB51C10"));
            settings.Background = new SolidColorBrush(Colors.White);
            settings.HeaderText =
                "Support";
            settings.IsOpen = true;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private static void ShowSettings()
        {
            SettingsFlyout settings = new SettingsFlyout();
            settings.Content = new Settings();
            settings.HeaderBrush = new SolidColorBrush(new HexColour("#FFB51C10"));
            settings.Background = new SolidColorBrush(Colors.White);
            settings.HeaderText =
                "Options";
            settings.IsOpen = true;
        }

        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //if (Config.ShowSettings)
            //{
            //    ShowSettings();
            //}
        }
    }
}
