using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Cineworld;
using System.Windows.Media.Imaging;
using System.Globalization;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Text;
using Cineworld.Services;

namespace CineWorld
{
    public partial class PersonDetails : PhoneApplicationPage
    {
        public static CastInfo castinfo { get; set; }
        Person p = null;

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

            SpeechSynthesisService.CancelExistingRequests();

            this.SpinAndWait(true);

            TMDBService tmdb = new TMDBService();
            
            if (App.TMDBConfig == null)
            {
                App.TMDBConfig = await tmdb.GetConfig();
            }

            p = await tmdb.GetPersonDetails(castinfo.ID);

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

            if(!Config.ShowCleanBackground && castinfo.ProfilePath != null)
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

        private async void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            Grid g = sender as Grid;

            MovieCastInfo cast = g.Tag as MovieCastInfo;

            if (cast != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(cast.Title);
                if(!String.IsNullOrWhiteSpace(cast.ReleaseDate))
                    sb.AppendFormat("released on {0}\n", cast.ReleaseDate);

                if (!String.IsNullOrWhiteSpace(cast.Character))
                    sb.AppendFormat("plays {0}\n", cast.Character);

                await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
            }
        }

        private async void piPoster_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Config.AudioSupport || p == null)
                return;

            if (!String.IsNullOrWhiteSpace(p.Biography))
            {
                await SpeechSynthesisService.SpeakOutLoud(p.Biography);
            }
        }

        private async void pMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            if (this.pMain.SelectedIndex == 0)
            {
                await SpeechSynthesisService.SpeakOutLoud("details of " + castinfo.Name);
            }
            else
            {
                await SpeechSynthesisService.SpeakOutLoud("appearanced in films listed");
            }
        }
    }
}