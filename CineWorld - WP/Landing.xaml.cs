using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;
using Cineworld;
using System.Device.Location;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Controls;
using WP7Helpers.Common;
using System.IO.IsolatedStorage;

namespace CineWorld
{
    enum CinemaTileType
    {
        Pinned,
        Favourite,
        Nearest
    }

    public partial class Landing : PhoneApplicationPage
    {
        public bool bLoaded = false;
        GeoCoordinateWatcher watcher = null;
        public static GeoPosition<GeoCoordinate> userPosition = null;
        HashSet<int> PinnedCinemas = new HashSet<int>();
        
        public Landing()
        {
            InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal, bool bErrored = false)
        {
            this.scWaiting.IsSpinning = bNewVal;

            if (!bErrored)
            {
                this.LayoutRoot.IsHitTestVisible = !bNewVal;
                this.LayoutRoot.Opacity = (bNewVal ? 0.5 : 1);
            }
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Config.ShowSettings)
            {
                NavigationService.Navigate(new Uri("/ConfigSettings.xaml", UriKind.Relative));
            }
            else
            {
                if (bLoaded)
                {
                    this.PinnedCinemas.Clear();
                    this.wpHubTiles.Children.Clear();
                    await this.LoadPinnedAndFavouriteCinemas();
                    return;
                }

                if (!Config.ShowCleanBackground)
                {
                    this.LayoutRoot.Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri("SplashScreenImage.jpg", UriKind.Relative)),
                        Opacity = 0.2,
                        Stretch = Stretch.UniformToFill
                    };
                }

                await ExecuteInitialDataLoad();

                bLoaded = true;
            }
        }

        private async Task ExecuteInitialDataLoad(bool bForce = false)
        {
            this.SpinAndWait(true);
            bool bErrored = false;

            PinnedCinemas.Clear();
            
            this.wpHubTiles.Children.Clear();
            
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
                MessageBox.Show("Error fetching cinema / film listing");
            }

            if (!bErrored)
            {
                await InstantiateTiles();
            }

            this.SpinAndWait(false, bErrored);
        }

        private async Task InstantiateTiles(bool bForce = false)
        {
            this.wpHubTiles.Children.Clear();

            await this.LoadFilmData();

            await this.LoadPinnedAndFavouriteCinemas(bForce);
        }

        private static async Task Initialise(bool bForce = false)
        {
            LocalStorageHelper lsh = new LocalStorageHelper();
            await lsh.DownloadFiles(bForce);

            await lsh.DeserialiseObjects();
        }

        char[] cSep = { '&', '=' };
        private async Task LoadPinnedAndFavouriteCinemas(bool bForce = false)
        {
            LocalStorageHelper lsh = new LocalStorageHelper();
            List<Task> cinemaDownloads = new List<Task>();
            
            foreach (var tile in ShellTile.ActiveTiles)
            {
                string[] parts = tile.NavigationUri.ToString().Split(cSep, StringSplitOptions.RemoveEmptyEntries);
                int iCin = -1;

                if (parts.Length == 2)
                {
                    // cinema id is second part
                    iCin = int.Parse(parts[1]);
                }
                else if (parts.Length == 4)
                {
                    // cinema id is second part
                    iCin = int.Parse(parts[1]);

                    if (Config.Region != (Config.RegionDef)int.Parse(parts[3]))
                        continue;
                }
                else
                    continue;

                PinnedCinemas.Add(iCin);
                cinemaDownloads.Add(lsh.GetCinemaFilmListings(iCin, bForce));
                var SelectedCinema = App.Cinemas[iCin];

                if (SelectedCinema != null)
                    this.SetTile(SelectedCinema, CinemaTileType.Pinned);
            }

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
                        this.SetTile(ci, CinemaTileType.Favourite);
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                }
            }

            if (Config.UseLocation)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(f =>
                {
                    watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default) { MovementThreshold = 10 };
                    watcher.StatusChanged += watcher_StatusChanged;
                    watcher.Start();
                }));
            }

            await TaskEx.WhenAll(cinemaDownloads);
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                LoadNearestCinema();
                watcher.Stop();
            }
        }

        private void LoadNearestCinema()
        {
            try
            {
                if (watcher != null)
                {
                    userPosition = watcher.Position;

                    IEnumerable<CinemaInfo> oc = App.Cinemas.Values.OrderBy(c => GeoMath.Distance(userPosition.Location.Latitude, userPosition.Location.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(1);
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
                                    this.Dispatcher.BeginInvoke(() =>
                                    {
                                        this.SetTile(ci, CinemaTileType.Nearest);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    message = ex.Message;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        static int tileIndex = 3;
        private void SetTile(CinemaInfo ci, CinemaTileType cinemaType)
        {
            if (ci == null)
                return;
            
            Tile t = new Tile() 
            { 
                CommandParameter = ci.ID, 
                Margin = new Thickness(12, 12, 0, 0), 
                Height = 210, 
                Width = 210, 
                Foreground = new SolidColorBrush(Colors.White), 
                Background = new ImageBrush() 
                { 
                    ImageSource = new BitmapImage(new Uri("Images/Background.png", UriKind.Relative)) 
                } 
            };

            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            TextBlock tbTopText = new TextBlock() { Text = cinemaType.ToString(), Margin = new Thickness(12), VerticalAlignment = System.Windows.VerticalAlignment.Top };
            Grid.SetColumn(tbTopText, 0);
            Grid.SetRow(tbTopText, 0);
            g.Children.Add(tbTopText);
            
            TextBlock tbBottomText = new TextBlock() { Margin = new Thickness(12), VerticalAlignment = System.Windows.VerticalAlignment.Bottom, TextWrapping = TextWrapping.Wrap };
            tbBottomText.Text = ci.Name;
            t.Click += ActiveTile_Click;
            
            Grid.SetColumn(tbBottomText, 0);
            Grid.SetRow(tbBottomText, 1);

            g.Children.Add(tbBottomText);

            t.Content = g;

            TurnstileFeatherEffect.SetFeatheringIndex(t, tileIndex++);

            this.wpHubTiles.Children.Add(t);
        }

        void ActiveTile_Click(object sender, RoutedEventArgs e)
        {
            Tile t = (sender as Tile);
            int iCin = (int)t.CommandParameter;

            var SelectedCinema = App.Cinemas[iCin];

            CinemaDetails.SelectedCinema = SelectedCinema;

            NavigationService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
        }

        private async Task LoadFilmData()
        {
            List<Uri> posterUrls = await new BaseStorageHelper().GetImageList();

            this.itFilms.ItemsSource = posterUrls;
            this.itFilms.IsFrozen = false;
        }

        void Tile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListCinemas.xaml", UriKind.Relative));
        }

        void ImageTile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListFilms.xaml", UriKind.Relative));
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ConfigSettings.xaml", UriKind.Relative));
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            this.SpinAndWait(true);

            try
            {
                List<Task> tasks = new List<Task>();

                await Initialise(true);

                foreach (var key in this.PinnedCinemas)
                {
                    tasks.Add(new LocalStorageHelper().GetCinemaFilmListings(key, true));
                }
                await TaskEx.WhenAll(tasks);

                this.SpinAndWait(false);
            }
            catch
            {
                this.SpinAndWait(false, true);
            }
        }

        private void btnManagePosters_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FilmPosters.xaml", UriKind.Relative));
        }
    }
}