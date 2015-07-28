using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Cineworld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewReviews : Page
    {
        public static FilmInfo SelectedFilm { get; set; }
        public static CinemaInfo SelectedCinema { get; set; }
        public static Cineworld.Review.ReviewTargetDef ReviewTarget { get; set; }

        public ViewReviews()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(ReviewTarget == Review.ReviewTargetDef.Film)
                this.lvReviews.ItemsSource =  SelectedFilm.Reviews;
            else
                this.lvReviews.ItemsSource = SelectedCinema.Reviews;
        }

        private void spWriteReview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Content = new Review();

            Review.ReviewTarget = ViewReviews.ReviewTarget;
            Review.SelectedFilm = ViewReviews.SelectedFilm;
            Review.SelectedCinema = ViewReviews.SelectedCinema;
        }
    }
}
