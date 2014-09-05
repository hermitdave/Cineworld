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
using System.Windows.Media.Imaging;

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

            if (bLoaded)
                return;

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("SplashScreenImage.jpg", UriKind.Relative)),
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

            FilmData cdCurrent = new FilmData(current);

            var dataLetter = cdCurrent.GetGroupsByLetter();

            this.lstMain.ItemsSource = dataLetter;

            FilmData cdUpcoming = new FilmData(upcomig);

            var dataLetterUpcoming = cdUpcoming.GetGroupsByLetter();

            this.lstUpcoming.ItemsSource = dataLetterUpcoming;

            bLoaded = true;
        }

        private void llsFilms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;
            if (lls.SelectedItem != null)
            {
                FilmDetails.SelectedFilm = (FilmInfo)lls.SelectedItem;
                lls.SelectionChanged -= new SelectionChangedEventHandler(llsFilms_SelectionChanged);
                lls.SelectedItem = null;
                lls.SelectionChanged += new SelectionChangedEventHandler(llsFilms_SelectionChanged);
                NavigationService.Navigate(new Uri("/FilmDetails.xaml", UriKind.Relative));
            }
        }

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }
    }
}