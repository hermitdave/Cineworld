using System;
using System.Collections.Generic;
using System.Globalization;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class PersonDetails : Cineworld.Common.LayoutAwarePage
    {
        public static CastInfo castinfo { get; set; }

        public PersonDetails()
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

        private void SpinAndWait(bool bNewVal)
        {
            this.gBody.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            this.SpinAndWait(true);

            TMDBService tmdb = new TMDBService();

            if (App.TMDBConfig == null)
            {
                App.TMDBConfig = await tmdb.GetConfig();
            }

            Person p = await tmdb.GetPersonDetails(castinfo.ID);

            this.LoadPersonDetails(p);

            this.SpinAndWait(false);
        }

        private void LoadPersonDetails(Person person)
        {
            if (person.AlsoKnownAs != null && person.AlsoKnownAs.Length > 0)
            {
                this.bcTitle.ViewTitle = String.Format("{0} ({1}", person.Name, person.AlsoKnownAs[0]);
            }
            else
                this.bcTitle.ViewTitle = person.Name;

            this.piPoster.Source = new BitmapImage(castinfo.ProfilePicture);

            if (!Config.ShowCleanBackground && castinfo.ProfilePath != null)
                this.gBody.Background = new ImageBrush() { ImageSource = new BitmapImage(castinfo.ProfilePath), Opacity = 0.2, Stretch = Stretch.Uniform };

            if (String.IsNullOrWhiteSpace(person.PlaceOfBirth))
                this.tbBirthPlaceTitle.Visibility = this.tbBirthPlace.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else
                tbBirthPlace.Text = person.PlaceOfBirth;

            if (String.IsNullOrWhiteSpace(person.Birthday))
                this.tbBirthDayTitle.Visibility = this.tbBirthDay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else
            {
                try
                {
                    DateTime dtBirthDay = DateTime.ParseExact(person.Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (dtBirthDay == DateTime.MinValue)
                        this.tbBirthDayTitle.Visibility = this.tbBirthDay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    else
                        this.tbBirthDay.Text = dtBirthDay.ToString("dd MMM yyyy");
                }
                catch { }
            }

            if (String.IsNullOrWhiteSpace(person.Deathday))
                this.tbDeathDayTitle.Visibility = this.tbDeathDay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else
            {
                try
                {
                    DateTime dtDeathday = DateTime.ParseExact(person.Deathday, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (dtDeathday == DateTime.MinValue)
                        this.tbDeathDayTitle.Visibility = this.tbDeathDay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    else
                        this.tbDeathDay.Text = dtDeathday.ToString("dd MMM yyyy");
                }
                catch { }
            }

            if (String.IsNullOrWhiteSpace(person.Homepage))
                this.hlbWebsiteTitle.Visibility = this.hlbWebsite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else
            {
                this.hlbWebsite.Content = person.Homepage;
                this.hlbWebsite.Tag = new Uri(person.Homepage);
            }

            if (!String.IsNullOrWhiteSpace(person.Biography))
                this.tbBiography.Text = person.Biography;

            if (person.Credits != null && person.Credits.Cast != null && person.Credits.Cast.Length > 0)
            {
                List<MovieCastInfo> movieRoles = new List<MovieCastInfo>();

                foreach (var movieRole in person.Credits.Cast.OrderByDescending(p => p.ReleaseDate))
                {
                    movieRoles.Add(new MovieCastInfo(App.TMDBConfig.Images.BaseUrl, "w185", movieRole));
                }

                this.lbMovies.ItemsSource = movieRoles;
            }
        }

        private void radShowBiography_Click(object sender, RoutedEventArgs e)
        {
            this.tbBiography.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.lbMovies.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void radShowMovies_Click(object sender, RoutedEventArgs e)
        {
            this.tbBiography.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.lbMovies.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }
}
