using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using Cineworld;
using System.Windows.Media.Imaging;
using System.Globalization;
using Microsoft.Phone.Tasks;

namespace CineWorld
{
    public partial class PersonDetails : PhoneApplicationPage
    {
        public static CastInfo castinfo { get; set; }

        public PersonDetails()
        {
            InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.scWaiting.IsSpinning = bNewVal;
            this.pMain.IsEnabled = !bNewVal;
            this.pMain.Opacity = (bNewVal ? 0 : 1);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
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
                this.pMain.Title = String.Format("{0} ({1}", person.Name, person.AlsoKnownAs[0]);
            }
            else
                this.pMain.Title = person.Name;

            this.piPoster.Source = new BitmapImage(castinfo.ProfilePicture);

            if (!Config.ShowCleanBackground && castinfo.ProfilePath != null)
                this.LayoutRoot.Background = new ImageBrush() { ImageSource = new BitmapImage(castinfo.ProfilePath), Stretch = Stretch.UniformToFill, Opacity = 0.2 };

            if (String.IsNullOrWhiteSpace(person.PlaceOfBirth))
                spBirthPlace.Visibility = System.Windows.Visibility.Collapsed;
            else
                tbBirthPlace.Text = person.PlaceOfBirth;

            if (String.IsNullOrWhiteSpace(person.Birthday))
                this.spBirthDay.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                try
                {
                    DateTime dtBirthDay = DateTime.ParseExact(person.Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (dtBirthDay == DateTime.MinValue)
                        this.spBirthDay.Visibility = System.Windows.Visibility.Collapsed;
                    else
                        this.tbBirthDay.Text = dtBirthDay.ToString("dd MMM yyyy");
                }
                catch { }
            }

            if (String.IsNullOrWhiteSpace(person.Deathday))
                this.spDeathDay.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                try
                {
                    DateTime dtDeathday = DateTime.ParseExact(person.Deathday, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (dtDeathday == DateTime.MinValue)
                        this.spDeathDay.Visibility = System.Windows.Visibility.Collapsed;
                    else
                        this.tbDeathDay.Text = dtDeathday.ToString("dd MMM yyyy");
                }
                catch { }
            }

            if (String.IsNullOrWhiteSpace(person.Homepage))
                this.spWebsite.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                this.hlbWebsite.Content = person.Homepage;
                this.hlbWebsite.Tag = new Uri(person.Homepage);
            }

            if (String.IsNullOrWhiteSpace(person.Biography))
                this.spBiography.Visibility = System.Windows.Visibility.Collapsed;
            else
                this.tbBiography.Text = person.Biography;

            if (person.Credits != null && person.Credits.Cast != null && person.Credits.Cast.Length > 0)
            {
                List<MovieCastInfo> movieRoles = new List<MovieCastInfo>();

                foreach (var movieRole in person.Credits.Cast.OrderByDescending(p => p.ReleaseDate))
                {
                    movieRoles.Add(new MovieCastInfo(App.TMDBConfig.Images.BaseUrl, "w185", movieRole));
                }

                this.lstMain.ItemsSource = movieRoles;
            }
        }

        private void hlbWebsite_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = sender as HyperlinkButton;

            Uri uri = (Uri)btn.Tag;
            WebBrowserTask wbt = new WebBrowserTask() { Uri = uri };
            wbt.Show();
        }
    }
}