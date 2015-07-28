using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
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
    public sealed partial class ListFilms : Cineworld.Common.LayoutAwarePage
    {
        internal enum FilmView
        {
            NotSet,
            Current,
            Upcoming,
        }

        FilmData filmData = new FilmData(App.Films);
        FilmView currentview = FilmView.NotSet;
        System.Collections.ObjectModel.ObservableCollection<GroupInfoList<object>> GroupedFilms = new System.Collections.ObjectModel.ObservableCollection<GroupInfoList<object>>();
        bool bLoaded = false;

        public ListFilms() : base(true)
        {
            this.InitializeComponent();
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
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

            DataTransferManager.GetForCurrentView().DataRequested += Default_DataRequested;

            gvZoomedInFilms.SelectionChanged -= gvZoomedIn_SelectionChanged;
            cvsFilms.Source = GroupedFilms;
            gvZoomedInFilms.SelectedItem = null;
            gvZoomedInFilms.SelectionChanged += gvZoomedIn_SelectionChanged;

            semanticZoom.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoom.ViewChangeStarted += semanticZoom_ViewChangeStarted;

            this.SetView(FilmView.Current);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            DataTransferManager.GetForCurrentView().DataRequested -= Default_DataRequested;
        }

        private void SetView(FilmView newCinemaView)
        {
            if (this.currentview == newCinemaView)
                return;

            this.currentview = newCinemaView;

            List<GroupInfoList<object>> filmDataSet = null;

            switch (this.currentview)
            {
                case FilmView.Current:
                    filmDataSet = filmData.CurrentFilmGroups;
                    break;

                case FilmView.Upcoming:
                    filmDataSet = filmData.UpcomingFilmGroups;
                    break;
            }

            if (filmDataSet != null)
            {
                GroupedFilms.Clear();

                foreach (var entry in filmDataSet)
                    GroupedFilms.Add(entry);

                (this.semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = filmData.GetFilmHeaders(filmDataSet);

                (this.semanticZoom.ZoomedInView as ListViewBase).UpdateLayout();

                GroupInfoList<object> group = filmDataSet.Find(g => g.Count > 0);

                if (group != null)
                    (this.semanticZoom.ZoomedInView as ListViewBase).ScrollIntoView(group);
            }
        }

        void Default_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = "Shall we watch a movie ?";

            args.Request.Data.SetText("Lets watch a movie... what shall we watch and where ? I've cineworld app open and waiting for your reply!");
        }

        async void gvZoomedIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageDialog md = new MessageDialog("selected");
            //await md.ShowAsync();
            if(this.gvZoomedInFilms.SelectedItem != null)
            {
                //FilmDetails.SelectedFilm = (FilmInfo)this.gvZoomedIn.SelectedItem;
                this.Frame.Navigate(typeof(FilmDetails), (FilmInfo)this.gvZoomedInFilms.SelectedItem);
            }
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

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.semanticZoom.IsZoomedInViewActive = false;
        }

        private void btnFilledViewOnly_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
        }

        private void radCurrent_Click(object sender, RoutedEventArgs e)
        {
            this.SetView(FilmView.Current);
        }

        private void radUpcoming_Click(object sender, RoutedEventArgs e)
        {
            this.SetView(FilmView.Upcoming);
        }
    }
}
