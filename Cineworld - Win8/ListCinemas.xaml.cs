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
    public sealed partial class ListCinemas : Cineworld.Common.LayoutAwarePage
    {
        List<GroupInfoList<object>> dataLetter = null;

        public ListCinemas()
        {
            this.InitializeComponent();

            CinemaData cd = new CinemaData(App.Cinemas);

            dataLetter = cd.GroupsByLetter; //cd.GetGroupsByLetter();
            
            cvsCinemas.Source = dataLetter;
            gvZoomedInCinemas.SelectionChanged -= gvZoomedIn_SelectionChanged;
            gvZoomedInCinemas.SelectedItem = null;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = cd.CinemaHeaders; //GetHeaderItems(dataLetter); //cvs2.View.CollectionGroups;
            gvZoomedInCinemas.SelectionChanged += gvZoomedIn_SelectionChanged;

            semanticZoom.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoom.ViewChangeStarted += semanticZoom_ViewChangeStarted;
        }

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

            //e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
        }

        public List<HeaderItem> GetHeaderItems(List<GroupInfoList<object>> data)
        {
            List<HeaderItem> items = new List<HeaderItem>();

            for (int i = 65; i <= 90; i++)
            {
                //if (i > 57 && i < 65)
                //    continue;

                char c = (char)i;

                if (data.Exists(k => ((char)k.Key) == c))
                    items.Add(new HeaderItem() { Name = c, IsEnabled = true });
                else
                    items.Add(new HeaderItem() { Name = c, IsEnabled = false });
            }

            return items;
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

        void gvZoomedIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.gvZoomedInCinemas.SelectedItem != null)
            {
                //CinemaDetails.SelectedCinema = (CinemaInfo)this.gvZoomedIn.SelectedItem;

                //CinemaDetails.CinemaID = ((CinemaInfo)this.gvZoomedIn.SelectedItem).ID;

                this.Frame.Navigate(typeof(CinemaDetails), ((CinemaInfo)this.gvZoomedInCinemas.SelectedItem).ID);
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
    }
}
