using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MyToolkit.Multimedia;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.Connectivity;
using Windows.Media.PlayTo;
using Windows.UI.Popups;
using Windows.System.Display;
using Telerik.UI.Xaml.Controls.Input;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ShowPerformances : Cineworld.Common.LayoutAwarePage
    {
        //CineWorldService cws = new CineWorldService();
        
        //static Dates SelectedDates { get; set; }
        YouTubeUri trailerUrl = null;
        
        public static CinemaInfo SelectedCinema { get; set; }
        public static FilmInfo SelectedFilm { get; set; }
        
        public static bool ShowCinemaDetails { get; set; }
        public static bool ShowFilmDetails { get; set; }
        
        public ShowPerformances()
        {
            this.InitializeComponent();
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataTransferManager.GetForCurrentView().DataRequested += FilmDetails_DataRequested;

            this.AllowSearch(false);

            PlayToManager.GetForCurrentView().SourceRequested += PlayToManagerOnSourceRequested;

            bool bError = false;
            try
            {
                SpinAndWait(true);

                this.piPoster.Source = new BitmapImage(SelectedFilm.PosterUrl);

                this.bcTitle.ViewTitle = String.Format("{0} at Cineworld {1}", SelectedFilm.TitleWithClassification, SelectedCinema.Name);

                if (!Config.ShowCleanBackground)
                {
                    if (SelectedFilm.BackdropUrl != null)
                    {
                        this.LayoutRoot.Background = new ImageBrush()
                        {
                            ImageSource = new BitmapImage(SelectedFilm.BackdropUrl),
                            Opacity = 0.2,
                            Stretch = Stretch.UniformToFill
                        };
                    }
                    else
                    {
                        string uri = (SelectedFilm.PosterImage.AbsoluteUri);
                        if (uri.Contains("w500"))
                        {
                            uri = uri.Replace("w500", "original");
                            this.LayoutRoot.Background = new ImageBrush()
                            {
                                ImageSource = new BitmapImage(new Uri(uri)),
                                Opacity = 0.2,
                                Stretch = Stretch.UniformToFill
                            };
                        }
                    }
                }

                this.DataContext = this;

                if (ShowCinemaDetails)
                {
                    await this.LoadCinemaDetails();
                    this.spCinemaButtons.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                
                if (ShowFilmDetails)
                {
                    await this.LoadFilmDetails();
                    this.spFilmButtons.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                
                PerformanceData perfData = new PerformanceData(SelectedFilm.Performances);
                this.cvsShowByDate.Source = perfData.GroupsByDate;

                semanticZoomShowByDate.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
                semanticZoomShowByDate.ViewChangeStarted += semanticZoom_ViewChangeStarted;

                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).ItemsSource = perfData.PerformanceHeaders;

            }
            catch
            {
                bError = true;
            }

            if (bError)
            {
                await new MessageDialog("Error downloading showtime data").ShowAsync();
            }

            SpinAndWait(false);

        }

        void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(DateHeaderItem))
            {
                DateHeaderItem hi = (DateHeaderItem)e.SourceItem.Item;

                if (hi.Group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = hi.Group };
            }
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        private async void PlayToManagerOnSourceRequested(PlayToManager sender, PlayToSourceRequestedEventArgs args)
        {
            var deferral = args.SourceRequest.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                args.SourceRequest.SetSource(this.mpTrailer.PlayToSource);

                deferral.Complete();
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= FilmDetails_DataRequested;

            PlayToManager.GetForCurrentView().SourceRequested -= PlayToManagerOnSourceRequested;

            base.OnNavigatedFrom(e);
        }

        PerformanceInfo perf = null;
        
        void FilmDetails_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = String.Format("\"{0}\" at Cineworld {1}", SelectedFilm.Title, SelectedCinema.Name);
                
            if (perf != null)
            {
                args.Request.Data.Properties.Description = (String.Format("Shall we go and see it {0} at {1}? Book here {2}", DateTimeToStringConverter.ConvertData(perf.PerformanceTS), perf.PerformanceTS.ToString("HH:mm"), perf.BookUrl.ToString()));
                args.Request.Data.SetWebLink(perf.BookUrl);

                perf = null;
            }
            else
            {
                args.Request.Data.Properties.Title = "movie tonight?";
                args.Request.Data.SetText(String.Format("Shall we go and see {0} at Cineworld {1}?", perf.FilmTitle, SelectedCinema.Name));
            }
        }

        private async Task LoadFilmDetails()
        {
            this.gFilmInfo.Visibility = Windows.UI.Xaml.Visibility.Visible;

            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            ConnectionCost InternetConnectionCost = (InternetConnectionProfile == null ? null : InternetConnectionProfile.GetConnectionCost());

            Task<YouTubeUri> tYouTube = null;
            if (!String.IsNullOrEmpty(SelectedFilm.YoutubeTrailer) &&
                InternetConnectionProfile != null &&
                InternetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess &&
                InternetConnectionCost != null &&
                InternetConnectionCost.NetworkCostType == NetworkCostType.Unrestricted)
            {
                tYouTube = MyToolkit.Multimedia.YouTube.GetVideoUriAsync(SelectedFilm.YoutubeTrailer, Config.TrailerQuality);
            }

            if (tYouTube != null)
                try
                {
                    this.trailerUrl = await tYouTube;
                }
                catch { }
            
            this.btnPlay.Visibility = this.btnPlayTrailer.Visibility = (trailerUrl != null ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed);
            this.mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async Task LoadCinemaDetails()
        {
            Task t = null;

            if (SelectedFilm.Performances == null || SelectedFilm.Performances.Count == 0)
            {
                t = new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID);
            }

            this.spCinemaInfo.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.radShowCast.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        
            this.spCinemaButtons.Visibility = Windows.UI.Xaml.Visibility.Visible;
            int iCin = SelectedCinema.ID;

            string cinemastr = SelectedCinema.ID.ToString();
            IReadOnlyList<SecondaryTile> tiles = await SecondaryTile.FindAllAsync();
            SecondaryTile tile = tiles.FirstOrDefault(s => s.TileId == cinemastr);

            if (tile == null)
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

            if (t != null)
            {
                await t;
                SelectedFilm = App.CinemaFilms[SelectedCinema.ID].Find(f => f.EDI == SelectedFilm.EDI);
            }
        }

        HexColour backgroundColor = new HexColour("#FF19171C");
        private DisplayRequest dispRequest = null;

        private void mpTrailer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (this.mpTrailer.CurrentState)
            {
                case MediaElementState.Opening:
                case MediaElementState.Buffering:
                case MediaElementState.Paused:
                    this.gBody.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;

                case MediaElementState.Playing:
                    if (dispRequest == null)
                    {
                        dispRequest = new DisplayRequest();
                        dispRequest.RequestActive();
                    }
                    this.gProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.prProgress.IsActive = false;
                    this.gBody.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;

                case MediaElementState.Stopped:
                    if (dispRequest != null)
                    {
                        dispRequest.RequestRelease();
                        dispRequest = null;
                    }
                    goto default;

                default:
                    this.mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.gBody.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;
            }
        }

        private void btnFilledViewOnly_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
        }

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            // Use the navigation frame to return to the previous page
            if (this.mpTrailer.Visibility == Windows.UI.Xaml.Visibility.Visible)
                this.mpTrailer.Stop();
            else if (this.Frame != null && this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private void radShowPerformances_Click(object sender, RoutedEventArgs e)
        {
            this.semanticZoomShowByDate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.lbCast.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void radShowCast_Click(object sender, RoutedEventArgs e)
        {
            this.semanticZoomShowByDate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.lbCast.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SemanticZoom sz = this.semanticZoomShowByDate;

            sz.IsZoomedInViewActive = false;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            this.gProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.prProgress.IsActive = true;
            this.gBody.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            mpTrailer.Source = this.trailerUrl.Uri;
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

            if (Cinemas.Contains(SelectedCinema.ID))
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

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PersonDetails.castinfo = (sender as Image).Tag as CastInfo;

            this.Frame.Navigate(typeof(PersonDetails));
        }

        private void RadFilmRating_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as RadRating).Value = SelectedFilm.AverageRating;
            
            Flyout flyOut = new Flyout();
            flyOut.Content = new ViewReviews();

            ViewReviews.ReviewTarget = Review.ReviewTargetDef.Film;
            ViewReviews.SelectedFilm = SelectedFilm;

            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }

        private void RadCinemaRating_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as RadRating).Value = SelectedCinema.AverageRating;
            
            Flyout flyOut = new Flyout();
            flyOut.Content = new ViewReviews();

            ViewReviews.ReviewTarget = Review.ReviewTargetDef.Cinema;
            ViewReviews.SelectedCinema = SelectedCinema;

            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }
    }
}
