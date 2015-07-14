using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Popups;
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
using Windows.System.Display;
using Windows.UI.ApplicationSettings;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class FilmDetails : Cineworld.Common.LayoutAwarePage
    {
        YouTubeUri trailerUrl = null;
        bool bLoaded = false;
        List<GroupInfoList<object>> dataLetter = null;
        public static FilmInfo SelectedFilm { get; set; }

        DispatcherTimer dtHideAppBar = new DispatcherTimer();

        public FilmDetails() : base(false)
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

        //private void SpinAndWait(bool bNewVal)
        //{
        //    this.gBody.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
        //    this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        //    this.prProgress.IsActive = bNewVal;
        //}
        
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataTransferManager.GetForCurrentView().DataRequested += FilmDetails_DataRequested;

            this.AllowSearch(false);

            PlayToManager.GetForCurrentView().SourceRequested += PlayToManagerOnSourceRequested;

            if (!bLoaded)
            {
                SelectedFilm = e.Parameter as FilmInfo; //App.Films[iFilm];

                this.piPoster.Source = new BitmapImage(SelectedFilm.PosterImage);

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
                            this.gBody.Background = new ImageBrush()
                            {
                                ImageSource = new BitmapImage(new Uri(uri)),
                                Opacity = 0.2,
                                Stretch = Stretch.UniformToFill
                            };
                        }
                    }
                }

                try
                {
                    await LoadFilmDetails();

                    this.dtHideAppBar.Interval = TimeSpan.FromSeconds(10);
                    this.dtHideAppBar.Tick += dtHideAppBar_Tick;
                    this.filmDetailsAppBar.IsOpen = true;
                    this.dtHideAppBar.Start();
                }
                catch
                {
                }

                bLoaded = true;
            }
        }

        void dtHideAppBar_Tick(object sender, object e)
        {
            this.dtHideAppBar.Tick -= dtHideAppBar_Tick;
            this.dtHideAppBar.Stop();

            this.filmDetailsAppBar.IsOpen = false;
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
            base.OnNavigatedFrom(e);

            DataTransferManager.GetForCurrentView().DataRequested -= FilmDetails_DataRequested;

            PlayToManager.GetForCurrentView().SourceRequested -= PlayToManagerOnSourceRequested;
        }

        void FilmDetails_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = String.Format("shall we see \"{0}\"", SelectedFilm.Title);

            args.Request.Data.SetText(SelectedFilm.Overview == null ? SelectedFilm.Title : SelectedFilm.Overview);
        }

        void gvZoomedIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.gvZoomedInCinemas.SelectedItem != null)
            {
                ShowPerformances.SelectedFilm = SelectedFilm;
                ShowPerformances.SelectedCinema = (CinemaInfo)this.gvZoomedInCinemas.SelectedItem;
                ShowPerformances.ShowFilmDetails = false;
                ShowPerformances.ShowCinemaDetails = true;
                this.Frame.Navigate(typeof(ShowPerformances));
            }
        }

        private async Task LoadFilmDetails()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile(); 

            ConnectionCost InternetConnectionCost = (InternetConnectionProfile == null ? null : InternetConnectionProfile.GetConnectionCost());

            Task<YouTubeUri> tYouTube = null;

            if (InternetConnectionProfile != null && 
                InternetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
            {
                if (!String.IsNullOrEmpty(SelectedFilm.YoutubeTrailer) &&
                InternetConnectionCost != null &&
                InternetConnectionCost.NetworkCostType == NetworkCostType.Unrestricted)
                {
                    tYouTube = MyToolkit.Multimedia.YouTube.GetVideoUriAsync(SelectedFilm.YoutubeTrailer, Config.TrailerQuality);
                }
            }

            this.DataContext = this;

            CinemaData cd = new CinemaData(App.FilmCinemas[SelectedFilm.EDI]);
            dataLetter = cd.GroupsByLetter; 
            cvsCinemas.Source = dataLetter;

            gvZoomedInCinemas.SelectionChanged -= gvZoomedIn_SelectionChanged;
            gvZoomedInCinemas.SelectedItem = null;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = cd.CinemaHeaders;
            gvZoomedInCinemas.SelectionChanged += gvZoomedIn_SelectionChanged;

            semanticZoom.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoom.ViewChangeStarted += semanticZoom_ViewChangeStarted;

            if (tYouTube != null)
            {
                try
                {
                    this.trailerUrl = await tYouTube;
                }
                catch { }
            }

            this.btnTrailer.Visibility = btnPlay.Visibility = (trailerUrl != null ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed);
            this.mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            //if(taskFilmReviews != null)
            //    await taskFilmReviews;
        }

        //async Task LoadFilmReviews(int FilmEdi)
        //{
        //    try
        //    {
        //        List<FilmReview> filmReviews = await App.MobileService.GetTable<FilmReview>().Where(r => r.Movie == SelectedFilm.EDI).ToListAsync();

        //        App.Reviews.Clear();

        //        if (filmReviews != null && filmReviews.Count > 0)
        //        {
        //            App.Reviews.AddRange(filmReviews);
        //            this.btnReviews.Visibility = Windows.UI.Xaml.Visibility.Visible;
        //        }

        //        this.filmRating.Value = App.AverageRating;
        //        this.tbFilmReviewCount.Text = String.Format("{0} votes", App.ReviewCount);

        //        this.spReviewButtons.Visibility = Windows.UI.Xaml.Visibility.Visible;
        //    }
        //    catch { }
        //}

        void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(HeaderItem))
            {
                HeaderItem hi = (HeaderItem)e.SourceItem.Item;

                var group = dataLetter.Find(d => ((char)d.Key) == hi.Name);
                if (group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = group };
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.semanticZoom.IsZoomedInViewActive = false;
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

        private void radShowCinemas_Click(object sender, RoutedEventArgs e)
        {
            this.semanticZoom.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.lbCast.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void radShowCast_Click(object sender, RoutedEventArgs e)
        {
            this.semanticZoom.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.lbCast.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            this.gProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.prProgress.IsActive = true;
            this.gBody.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            mpTrailer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            mpTrailer.Source = this.trailerUrl.Uri;
        }

        private void btnTrailer_Click(object sender, RoutedEventArgs e)
        {
            this.btnPlay_Click(sender, e);
        }

        private void btnRateFilm_Click(object sender, RoutedEventArgs e)
        {
            Flyout flyOut = new Flyout();
            flyOut.Content = new Review();

            Review.ReviewTarget = Review.ReviewTargetDef.Film;
            Review.SelectedFilm = SelectedFilm;

            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }

        private void btnReviews_Click(object sender, RoutedEventArgs e)
        {
            Flyout flyOut = new Flyout();
            flyOut.Content = new ViewReviews();

            ViewReviews.ReviewTarget = Review.ReviewTargetDef.Film;
            ViewReviews.SelectedFilm = SelectedFilm;

            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PersonDetails.castinfo = (sender as Image).Tag as CastInfo;

            this.Frame.Navigate(typeof(PersonDetails));
        }
    }
}
