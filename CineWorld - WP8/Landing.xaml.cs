using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;
using Cineworld;
using System.Device.Location;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using Windows.Phone.System.UserProfile;
using System.Globalization;
using Nokia.Music;
using System.IO;
using WP8_BackgroundTask;
using System.IO.IsolatedStorage;
using System.Text;
using CineWorld.Services;
using Cineworld.Services;
//using Cimbalino.Phone.Toolkit.Services;
//using Cimbalino.Phone.Toolkit.Helpers;

namespace CineWorld
{
    public partial class Landing : PhoneApplicationPage
    {
        enum CinemaTileType
        {
            Pinned,
            Favourite,
            Nearest
        }

        //GeoCoordinateWatcher watcher = null;
        HashSet<int> PinnedCinemas = new HashSet<int>();
        public static GeoCoordinate userPosition = null;
        
        bool bLoaded = false;
        
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

            SpeechSynthesisService.CancelExistingRequests();

            //if (Config.ShowSettings)
            //{
            //    NavigationService.Navigate(new Uri("/ConfigSettings.xaml", UriKind.Relative));
            //    return;
            //}
            //else
            {
                if (!Config.ShowCleanBackground)
                {
                    this.LayoutRoot.Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri("SplashScreenImage-WVGA.jpg", UriKind.Relative)),
                        Opacity = 0.2,
                        Stretch = Stretch.UniformToFill
                    };
                }
                else
                {
                    this.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
                }

                if (bLoaded)
                {
                    this.PinnedCinemas.Clear();
                    this.wpHubTiles.Children.Clear();
                    await this.LoadPinnedAndFavouriteCinemas();
                    return;
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
            try
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
                    Geolocator locator = null;

                    try
                    {
                        locator = new Geolocator();

                        var pos = await locator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

                        if (pos != null && pos.Coordinate != null)
                        {
                            userPosition = pos.Coordinate.ToGeoCoordinate();

                            this.LoadNearestCinema(cinemaDownloads, lsh);
                        }
                    }
                    catch { }
                }

                if (cinemaDownloads.Count > 0)
                    await Task.WhenAll(cinemaDownloads);
            }
            catch { }
        }

        //void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        //{
        //    if (e.Status == GeoPositionStatus.Ready)
        //    {
        //        LoadNearestCinema();
        //        watcher.Stop();
        //    }
        //}

        private void LoadNearestCinema(List<Task> cinemaDownloads, LocalStorageHelper lsh)
        {
            if(userPosition == null || (userPosition.Latitude == 0 && userPosition.Longitude == 0))
                return;

            try
            {
                IEnumerable<CinemaInfo> filteredCinemas = App.Cinemas.Values.Where(c => c.Longitute != 0 && c.Longitute != 0 && !PinnedCinemas.Contains(c.ID));

                if (!filteredCinemas.Any())
                    return;

                IEnumerable<CinemaInfo> oc = filteredCinemas.OrderBy(c => GeoMath.Distance(userPosition.Latitude, userPosition.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(1);
                if (oc.Any())
                {
                    int iCin = oc.First().ID;

                    lock (PinnedCinemas)
                    {
                        if (!PinnedCinemas.Contains(iCin))
                        {
                            if (App.Cinemas.ContainsKey(iCin))
                            {
                                PinnedCinemas.Add(iCin);
                                cinemaDownloads.Add(lsh.GetCinemaFilmListings(iCin, true));

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

        static int tileIndex = 2;
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
            t.Tap += t_Tap;
            
            Grid.SetColumn(tbBottomText, 0);
            Grid.SetRow(tbBottomText, 1);
            
            g.Children.Add(tbBottomText);

            t.Content = g;

            TurnstileFeatherEffect.SetFeatheringIndex(t, tileIndex++);

            this.wpHubTiles.Children.Add(t);
        }

        private async Task LoadFilmData()
        {
            List<Uri> posterUrls = await new BaseStorageHelper().GetImageList();

            this.itFilms.ItemsSource = posterUrls;

            this.itFilms.IsFrozen = false;
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ConfigSettings.xaml", UriKind.Relative));
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
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

        private async void btnDeleteCurrent_Click(object sender, EventArgs e)
        {
            string currentLockscreen = Config.CurrentLockscreen;

            List<Uri> images = await new BaseStorageHelper().GetImageList();

            List<Uri> otherimages = images.FindAll(i => String.Compare(Path.GetFileName(i.AbsoluteUri), currentLockscreen, StringComparison.OrdinalIgnoreCase) != 0);

            await ScheduledAgent.LockScreenChange(otherimages);

            if (!String.IsNullOrWhiteSpace(currentLockscreen))
            {
                try
                {
                    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                    string folder = "Shared\\ShellContent";

                    string fileToDelete = String.Format("{0}\\{1}", folder, currentLockscreen);

                    isf.DeleteFile(fileToDelete);
                }
                catch { }
            }
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            new BaseStorageHelper().CleanSharedContent();
        }

        private async Task SpeakOutLoud(object sender)
        {
            ButtonBase buttonbase = (sender as ButtonBase);
            Grid g = buttonbase.Content as Grid;

            StringBuilder sb = new StringBuilder();
            foreach (var child in g.Children)
            {
                if (child is TextBlock)
                    sb.AppendLine((child as TextBlock).Text);
            }

            if (sb.Length > 0)
            {
                await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
            }
        }

        private async void itFilms_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ButtonBase buttonbase = sender as ButtonBase;
            if (Config.AudioSupport && buttonbase.Tag == null)
            {
                buttonbase.Tag = DateTime.Now;
                await SpeakOutLoud(sender);
                e.Handled = true;

                return;
            }
            else
            {
                NavigationService.Navigate(new Uri("/ListFilms.xaml", UriKind.Relative));
            }
        }

        private async void tCinemas_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ButtonBase buttonbase = sender as ButtonBase;
            if (Config.AudioSupport && buttonbase.Tag == null)
            {
                buttonbase.Tag = DateTime.Now;
                await SpeakOutLoud(sender);
                e.Handled = true;

                return;
            }
            else
            {
                NavigationService.Navigate(new Uri("/ListCinemas.xaml", UriKind.Relative));
            }
        }

        private async void t_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ButtonBase buttonbase = sender as ButtonBase;
            if (Config.AudioSupport && buttonbase.Tag == null)
            {
                buttonbase.Tag = DateTime.Now;
                await SpeakOutLoud(sender);
                e.Handled = true;

                return;
            }
            else
            {
                Tile t = (sender as Tile);
                int iCin = (int)t.CommandParameter;

                var SelectedCinema = App.Cinemas[iCin];

                CinemaDetails.SelectedCinema = SelectedCinema;

                NavigationService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.Relative));
        }
    }
}