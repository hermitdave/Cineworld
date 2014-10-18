using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using System.Windows.Media.Animation;
using Cineworld;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Cineworld.Services;
using System.Threading.Tasks;
using System.Text;

namespace CineWorld
{
    public partial class ListFilms : PhoneApplicationPage
    {
        bool bLoaded = false;
        
        public ListFilms()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SpeechSynthesisService.CancelExistingRequests();

            if (bLoaded)
                return;

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("SplashScreenImage-WVGA.jpg", UriKind.Relative)),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            List<FilmInfo> current = new List<FilmInfo>();
            List<FilmInfo> upcomig = new List<FilmInfo>();

            foreach (var film in App.Films.Values)
            {
                if (film.Release <= DateTime.UtcNow)
                    current.Add(film);
                else
                    upcomig.Add(film);
            }

            this.lstMain.IsGroupingEnabled = this.lstUpcoming.IsGroupingEnabled = Config.GroupData;

            if (Config.GroupData)
            {
                FilmData cdCurrent = new FilmData(current);

                var dataLetter = cdCurrent.GetGroupsByLetter();

                this.lstMain.ItemsSource = dataLetter.ToList();

                FilmData cdUpcoming = new FilmData(upcomig);

                var dataLetterUpcoming = cdUpcoming.GetGroupsByLetter();

                this.lstUpcoming.ItemsSource = dataLetterUpcoming.ToList();
            }
            else
            {
                this.lstMain.ItemsSource = current;
                this.lstUpcoming.ItemsSource = upcomig;
            }

            bLoaded = true;
        }

        //private void llsFilms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LongListSelector lls = sender as LongListSelector;
        //    if (lls.SelectedItem != null)
        //    {
        //        FilmDetails.SelectedFilm = (FilmInfo)lls.SelectedItem;
        //        lls.SelectionChanged -= new SelectionChangedEventHandler(llsFilms_SelectionChanged);
        //        lls.SelectedItem = null;
        //        lls.SelectionChanged += new SelectionChangedEventHandler(llsFilms_SelectionChanged);
        //        NavigationService.Navigate(new Uri("/FilmDetails.xaml", UriKind.Relative));
        //    }
        //}

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private async void pMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            if(this.pMain.SelectedItem == null)
                return;

            PivotItem pi = this.pMain.SelectedItem as PivotItem;
            if(pi == null)
                return;

            await SpeechSynthesisService.SpeakOutLoud((string)pi.Header + " films");
        }

        FilmInfo fi = null;
        private async void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid g = (sender as Grid);
            FilmInfo newfilm = (g.Tag as FilmInfo);
            if (Config.AudioSupport && fi != newfilm)
            {
                fi = newfilm;

                await SpeakFilmInfo(fi);

                return;
            }
            else
            {
                FilmDetails.SelectedFilm = newfilm;
                NavigationService.Navigate(new Uri("/FilmDetails.xaml", UriKind.Relative));
            }
        }

        private async Task SpeakFilmInfo(FilmInfo fi)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fi.Title);
            sb.AppendFormat("classification {0}\n", fi.Classification);
            
            //if(fi.VoteCount > 0)
            //    sb.AppendFormat("average rating {0}, {1} reviews\n", fi.AverageRating.ToString("N2"), fi.VoteCount);
            
            //sb.AppendFormat("release date {0}\n", fi.Release.ToLongDateString());
            
            //if(fi.Runtime > 0)
            //    sb.AppendFormat("duration {0} minutes\n", fi.Runtime);
            
            //sb.AppendFormat("short description {0}", fi.Overview);

            await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
        }
    }
}