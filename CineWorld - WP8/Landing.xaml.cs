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
using Telerik.Windows.Controls;

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

        HashSet<int> PinnedCinemas = new HashSet<int>();
        public static GeoCoordinate userPosition = null;
        
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

            await ExecuteInitialDataLoad();
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

            await LoadCinemasByLocation();
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

                if (cinemaDownloads.Count > 0)
                    await Task.WhenAll(cinemaDownloads);
            }
            catch { }
        }

        private async Task LoadCinemasByLocation()
        {
            if (Config.UseLocation)
            {
                Geolocator locator = null;

                if (userPosition == null)
                {
                    try
                    {
                        locator = new Geolocator();

                        var pos = await locator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

                        if (pos != null && pos.Coordinate != null)
                        {
                            try
                            {
                                FlurryWP8SDK.Api.SetLocation(pos.Coordinate.Latitude, pos.Coordinate.Longitude, (float)pos.Coordinate.Accuracy);
                            }
                            catch { }

                            userPosition = pos.Coordinate.ToGeoCoordinate();

                            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
                            {
                                userPosition = new GeoCoordinate(51.5072, -0.1275);
                            }
                        }
                    }
                    catch { }
                }

                if (userPosition != null)
                {
                    this.LoadNearestCinemas();
                }
            }
        }

        private void LoadNearestCinemas()
        {
            if(userPosition == null || (userPosition.Latitude == 0 && userPosition.Longitude == 0))
                return;

            try
            {
                IEnumerable<CinemaInfo> filteredCinemas = App.Cinemas.Values.Where(c => c.Longitute != 0 && c.Longitute != 0 && !PinnedCinemas.Contains(c.ID));

                if (!filteredCinemas.Any())
                    return;

                int MaxCinemaCount = 9;

                int CountRequest = MaxCinemaCount - PinnedCinemas.Count;

                IEnumerable<CinemaInfo> cinemasByLocation = filteredCinemas.OrderBy(c => GeoMath.Distance(userPosition.Latitude, userPosition.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(CountRequest);
                foreach(var cinema in cinemasByLocation)
                {
                    var distance = GeoMath.Distance(userPosition.Latitude, userPosition.Longitude, cinema.Latitude, cinema.Longitute, GeoMath.MeasureUnits.Miles);
                    
                    if (distance > 50)
                        continue;

                    int iCin = cinema.ID;

                    lock (PinnedCinemas)
                    {
                        if (!PinnedCinemas.Contains(iCin))
                        {
                            if (App.Cinemas.ContainsKey(iCin))
                            {
                                PinnedCinemas.Add(iCin);
                                //cinemaDownloads.Add(lsh.GetCinemaFilmListings(iCin, bForce));

                                CinemaInfo ci = App.Cinemas[iCin];
                                string message = null;
                                try
                                {
                                    this.Dispatcher.BeginInvoke(() =>
                                    {
                                        this.SetTile(ci, CinemaTileType.Nearest, distance);
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
        private void SetTile(CinemaInfo ci, CinemaTileType cinemaType, double distance = 0)
        {
            if (ci == null)
                return;

            Grid gFront = new Grid();
            
            Image img = new Image() { Source = new BitmapImage(new Uri("Images/CineTileFront.png", UriKind.Relative)) };
            gFront.Children.Add(img);

            TextBlock tbTitle = new TextBlock() { Margin = new Thickness(6), Text = ci.Name, TextWrapping = TextWrapping.Wrap, VerticalAlignment = System.Windows.VerticalAlignment.Bottom};
            Grid.SetRow(tbTitle, 1);
            gFront.Children.Add(tbTitle);

            switch (cinemaType)
            {
                case CinemaTileType.Nearest:
                    gFront.Children.Add(new TextBlock() { Margin = new Thickness(6), Text = String.Format("{0:N2} miles", distance), TextWrapping = TextWrapping.Wrap, VerticalAlignment = System.Windows.VerticalAlignment.Top });
                    break;

                default:
                    gFront.Children.Add(new TextBlock() { Margin = new Thickness(6), Text = cinemaType.ToString(), TextWrapping = TextWrapping.Wrap, VerticalAlignment = System.Windows.VerticalAlignment.Top });
                    break;
            }

            RadCustomHubTile t = new RadCustomHubTile() 
            { 
                CommandParameter = ci.ID, 
                Margin = new Thickness(12, 12, 0, 0), 
                Height = 144, 
                Width = 144, 
                FrontContent =  gFront
            };

            t.Tap += t_Tap;
            
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
            StringBuilder sb = new StringBuilder();
                
            if(sender is ButtonBase)
            { 
                ButtonBase buttonbase = (sender as ButtonBase);
                Grid g = buttonbase.Content as Grid;

                GetTextContent(sb, g);

                
            }
            else if (sender is HubTileBase)
            {
                HubTileBase t = sender as HubTileBase;
                sb.AppendLine(t.Name);
                //GetTextContent(sb, t.Name as Grid);
                //GetTextContent(sb, t.BackContent as Grid);
            }

            if (sb.Length > 0)
            {
                await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
            }
        }

        private static void GetTextContent(StringBuilder sb, Grid g)
        {
            foreach (var child in g.Children)
            {
                if (child is TextBlock)
                    sb.AppendLine((child as TextBlock).Text);
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
            HubTileBase t = sender as HubTileBase;
            if (Config.AudioSupport && t.Tag == null)
            {
                t.Tag = DateTime.Now;
                await SpeakOutLoud(sender);
                e.Handled = true;

                return;
            }
            else
            {
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