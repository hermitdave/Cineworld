using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace Cineworld
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResults : Cineworld.Common.LayoutAwarePage
    {
        Dictionary<string, FilmInfo> searchFilms = new Dictionary<string, FilmInfo>();
        Dictionary<string, CinemaInfo> searchCinemas = new Dictionary<string, CinemaInfo>();
        bool bAllowNav = false;

        public SearchResults()
        {
            this.InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.Cinemas == null || App.Cinemas.Count == 0)
            {
                this.SpinAndWait(true);

                LocalStorageHelper lsh = new LocalStorageHelper();
                await lsh.DownloadFiles();

                await lsh.DeserialiseObjects();

                this.SpinAndWait(false);
            }

            base.OnNavigatedTo(e);
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
            var queryText = navigationParameter as String;

            List<SearchResult> matches = new List<SearchResult>();

            foreach (var f in App.Films.Values)
            {
                IEnumerable<CastInfo> casts = from cast in f.FilmCast
                                              where cast.Name.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase)
                                              select cast;

                foreach (var c in casts)
                {
                    //searchCinemas.Add(c.Name, c);
                    matches.Add(new SearchResult() { Name = c.Name, Subtitle = String.Format("{0} in {1}", c.Character, f.Title), SearchObject = f, Image = c.ProfilePath });
                }
            }

            IEnumerable<FilmInfo> films = from film in App.Films.Values
                                          where film.Title.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase)
                                          select film;

            searchFilms.Clear();
            searchCinemas.Clear();

            foreach (var f in films)
            {
                searchFilms.Add(f.Title, f);
                matches.Add(new SearchResult() { Name = f.Title, Image = f.PosterUrl, SearchObject = f });
            }

            IEnumerable<CinemaInfo> cinemas = from cinema in App.Cinemas.Values
                                                where cinema.Name.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase)
                                                select cinema;

            foreach (var c in cinemas)
            {
                searchCinemas.Add(c.Name, c);
                matches.Add(new SearchResult() { Name = c.Name, SearchObject = c, Image = new Uri("ms-appx:///Assets/Background.png") });
            }
            
            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<Filter>();
            filterList.Add(new Filter("All", 0, true));

            // Communicate results through the view model
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';

            bAllowNav = false;
            this.resultsGridView.ItemClick -= resultsGridView_ItemClick;
            this.DefaultViewModel["Results"] = matches;

            //this.resultsGridView.SelectedItem = null;

            this.resultsGridView.ItemClick += resultsGridView_ItemClick;
            bAllowNav = true;

            //this.DefaultViewModel["Filters"] = filterList;
            //this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;
        }

        private void btnFilledViewOnly_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                // TODO: Respond to the change in active filter by setting this.DefaultViewModel["Results"]
                //       to a collection of items with bindable Image, Title, Subtitle, and Description properties

                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : Cineworld.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }

        private void resultsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem != null && bAllowNav)
            {
                SearchResult res = (SearchResult)e.ClickedItem;
            
                if (res.SearchObject is FilmInfo)
                {
                    //FilmDetails.SelectedFilm = (res.SearchObject as FilmInfo);
                    this.Frame.Navigate(typeof(FilmDetails), res.SearchObject);
                }
                else if (res.SearchObject is CinemaInfo)
                {
                    //CinemaDetails.SelectedCinema = (res.SearchObject as CinemaInfo);
                    this.Frame.Navigate(typeof(CinemaDetails), (res.SearchObject as CinemaInfo).ID);
                }
            }
        }
    }
}
