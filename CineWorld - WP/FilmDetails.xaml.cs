using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using MyToolkit.Multimedia;
using Cineworld;
using Microsoft.Phone.Shell;
using Coding4Fun.Toolkit.Controls;

namespace CineWorld
{
    public partial class FilmDetails : PhoneApplicationPage
    {
        bool bLoaded = false;
        
        public static FilmInfo SelectedFilm { get; set; }

        ApplicationBarIconButton abibTrailer = null;
        ApplicationBarIconButton abibShare = null;

        public FilmDetails()
        {
            InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.scWaiting.IsSpinning = bNewVal;
            this.pMain.IsEnabled = !bNewVal;
            this.pMain.Opacity = (bNewVal ? 0.5 : 1);
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (bLoaded)
                return;

            this.abibTrailer = new ApplicationBarIconButton() { Text = "trailer", IconUri = new Uri("/Images/appbar.transport.play.rest.png", UriKind.Relative) };
            this.abibTrailer.Click += this.btnViewTrailer_Click;

            this.abibShare = new ApplicationBarIconButton() { Text = "share", IconUri = new Uri("/Images/appbar.share03.png", UriKind.Relative) };
            this.abibShare.Click += this.btnShare_Click;

            if (!Config.ShowCleanBackground && SelectedFilm.MediumPosterUrl != null)
            {
                this.pMain.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(SelectedFilm.MediumPosterUrl),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }
         
            LoadFilmDetails();

            this.ApplicationBar.Buttons.Clear();

            if (!String.IsNullOrWhiteSpace(SelectedFilm.YoutubeTrailer))
                this.ApplicationBar.Buttons.Add(this.abibTrailer);

            this.ApplicationBar.Buttons.Add(this.abibShare);
            
            YouTube.CancelPlay();

            bLoaded = true;
        }

        private void LoadFilmDetails()
        {
            this.DataContext = SelectedFilm;
            
            if (!String.IsNullOrWhiteSpace(SelectedFilm.YoutubeTrailer))
                this.btnPlay.Visibility = System.Windows.Visibility.Visible;
            else
                this.btnPlay.Visibility = System.Windows.Visibility.Collapsed;
            
            CinemaData cd = new CinemaData(App.FilmCinemas[SelectedFilm.EDI]);
            var dataLetter = cd.GetGroupsByLetter();
            lstMain.ItemsSource = dataLetter;
        }
        
        void llsCinemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lstMain.SelectedItem != null)
            {
                ShowPerformances.SelectedCinema = App.Cinemas[((CinemaInfo)this.lstMain.SelectedItem).ID];
                this.lstMain.SelectionChanged -= new SelectionChangedEventHandler(llsCinemas_SelectionChanged);
                this.lstMain.SelectedItem = null;
                this.lstMain.SelectionChanged += new SelectionChangedEventHandler(llsCinemas_SelectionChanged);
                ShowPerformances.SelectedFilm = SelectedFilm;
                ShowPerformances.ShowCinemaDetails = true;
                ShowPerformances.ShowFilmDetails = false;
                NavigationService.Navigate(new Uri("/ShowPerformances.xaml", UriKind.Relative));
            }
        }

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }        
        
        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (YouTube.CancelPlay())
                e.Cancel = true;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            YouTube.Play(SelectedFilm.YoutubeTrailer);
        }

        private void btnViewTrailer_Click(object sender, EventArgs e)
        {
            YouTube.Play(SelectedFilm.YoutubeTrailer);
        }

        private void lbCast_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbCast = sender as ListBox;

            if (lbCast.SelectedItem != null)
            {
                PersonDetails.castinfo = (CastInfo)lbCast.SelectedItem;

                lbCast.SelectedIndex = -1;

                NavigationService.Navigate(new Uri("/PersonDetails.xaml", UriKind.Relative));
            }
        }

        private void btnReview_Click(object sender, EventArgs e)
        {

        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            this.SetContextMenu();
        }

        private void filmRating_ValueChanged(object sender, EventArgs e)
        {
            RateAndReview();
        }

        private void RateAndReview()
        {
            Review.SelectedFilm = FilmDetails.SelectedFilm;
            Review.ReviewTarget = Review.ReviewTargetDef.Film;

            NavigationService.Navigate(new Uri("/Review.xaml", UriKind.Relative));
        }

        private void bRateReview_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.RateAndReview();
        }

        private void SetContextMenu()
        {
            string message = String.Format("Shall we go and see \"{0}\" at cineworld?", SelectedFilm.Title);
       
            new AppBarPrompt(
                new AppBarPromptAction("sms", () =>
                {
                    new SmsComposeTask()
                    {
                        Body = message
                    }.Show();
                }),
                new AppBarPromptAction("email", () =>
                {
                    new EmailComposeTask()
                    {
                        Body = message
                    }.Show();
                }),
                new AppBarPromptAction("social networks", () =>
                {
                    new ShareStatusTask()
                    {
                        Status = message
                    }.Show();
                })
                ) { Foreground = new SolidColorBrush(Colors.White) }.Show();
        }
    }
}