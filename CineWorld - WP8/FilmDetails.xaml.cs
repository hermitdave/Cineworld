using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Cineworld;
using MyToolkit.Multimedia;
using System.Threading.Tasks;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Tasks;
using Nokia.Music.Tasks;
using System.Text;
using Cineworld.Services;

namespace CineWorld
{
    public partial class FilmDetails : PhoneApplicationPage
    {
        bool bLoaded = false;

        public static FilmInfo SelectedFilm { get; set; }

        ApplicationBarIconButton abibTrailer = null;
        ApplicationBarIconButton abibShare = null;
        ApplicationBarIconButton abibSoundTrack = null;

        public FilmDetails()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SpeechSynthesisService.CancelExistingRequests();

            if (bLoaded)
                return;

            int iFilm = int.MinValue;

            if (this.NavigationContext.QueryString.ContainsKey("FilmID") && SelectedFilm == null)
            {
                iFilm = int.Parse(this.NavigationContext.QueryString["FilmID"]);
            }
            else
            {
                iFilm = SelectedFilm.EDI;
            }

            bool bError = false;
            try
            {
                if (App.Films == null || App.Films.Count == 0)
                {
                    LocalStorageHelper lsh = new LocalStorageHelper();
                    await lsh.DownloadFiles(false);

                    await lsh.DeserialiseObjects();
                }

                SelectedFilm = App.Films[iFilm];
            }
            catch (Exception ex)
            {
                bError = true;
            }

            if (bError)
            {
                MessageBox.Show("Error fetching film details");
            }
            
            this.abibTrailer = new ApplicationBarIconButton() { Text = "trailer", IconUri = new Uri("/Images/appbar.transport.play.rest.png", UriKind.Relative) };
            this.abibTrailer.Click += this.btnViewTrailer_Click;

            this.abibShare = new ApplicationBarIconButton() { Text = "share", IconUri = new Uri("/Images/appbar.share03.png", UriKind.Relative) };
            this.abibShare.Click += this.btnShare_Click;

            this.abibSoundTrack = new ApplicationBarIconButton() { IconUri = new Uri("/Images/appbar.notes.rest.png", UriKind.Relative), Text = "sounds track" };
            this.abibSoundTrack.Click += abibSoundTrack_Click;
            
            if (!Config.ShowCleanBackground)
            {
                if (SelectedFilm.BackdropUrl != null)
                {
                    this.pMain.Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(SelectedFilm.BackdropUrl),
                        Opacity = 0.2,
                        Stretch = Stretch.UniformToFill
                    };
                }
                else if (SelectedFilm.MediumPosterUrl != null)
                {
                    this.LayoutRoot.Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(SelectedFilm.MediumPosterUrl),
                        Opacity = 0.2,
                        Stretch = Stretch.UniformToFill
                    };
                }
            }

            LoadFilmDetails();

            this.ApplicationBar.Buttons.Clear();

            if (!String.IsNullOrWhiteSpace(SelectedFilm.YoutubeTrailer))
                this.ApplicationBar.Buttons.Add(this.abibTrailer);

            this.ApplicationBar.Buttons.Add(this.abibSoundTrack);

            this.ApplicationBar.Buttons.Add(this.abibShare);

            YouTube.CancelPlay();

            bLoaded = true;
        }

        private void SetMusicSearchMenu()
        {
            string searchterm = String.Format("{0} soundtrack", SelectedFilm.CleanTitle);

            new AppBarPrompt(
                new AppBarPromptAction("XBox Music", () =>
                {
                    new MarketplaceSearchTask()
                    {
                        ContentType = MarketplaceContentType.Music,
                        SearchTerms = searchterm
                    }.Show();

                }),
                new AppBarPromptAction("Nokia Music", () =>
                {
                    new MusicSearchTask()
                    {
                        SearchTerms = searchterm
                    }.Show();
                })
                ) { Foreground = new SolidColorBrush(Colors.White) }.Show();
        }

        void abibSoundTrack_Click(object sender, EventArgs e)
        {
            if (Config.AllowNokiaMusicSearch == true)
                SetMusicSearchMenu();
            else
            {
                string searchterm = String.Format("{0} soundtrack", SelectedFilm.CleanTitle);

                new MarketplaceSearchTask()
                {
                    ContentType = MarketplaceContentType.Music,
                    SearchTerms = searchterm
                }.Show();
            }
        }

        private void LoadFilmDetails()
        {
            this.DataContext = SelectedFilm;
            
            //if (!String.IsNullOrWhiteSpace(SelectedFilm.YoutubeTrailer))
            //    this.btnPlay.Visibility = System.Windows.Visibility.Visible;
            //else
            //    this.btnPlay.Visibility = System.Windows.Visibility.Collapsed;
            
            CinemaData cd = new CinemaData(App.FilmCinemas[SelectedFilm.EDI]);
            var dataLetter = cd.GetGroupsByLetter();
            lstMain.ItemsSource = dataLetter.ToList();

            if(SelectedFilm.FilmCast == null || SelectedFilm.FilmCast.Count == 0)
            {
                this.pMain.Items.Remove(this.piCast);
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

        private void lstMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lstMain.SelectedItem != null)
            {
                ShowPerformances.SelectedCinema = App.Cinemas[((CinemaInfo)this.lstMain.SelectedItem).ID];
                this.lstMain.SelectionChanged -= new SelectionChangedEventHandler(lstMain_SelectionChanged);
                this.lstMain.SelectedItem = null;
                this.lstMain.SelectionChanged += new SelectionChangedEventHandler(lstMain_SelectionChanged);
                ShowPerformances.SelectedFilm = SelectedFilm;
                ShowPerformances.ShowCinemaDetails = true;
                ShowPerformances.ShowFilmDetails = false;
                NavigationService.Navigate(new Uri("/ShowPerformances.xaml", UriKind.Relative));
            }
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
            if (this.lbCast.SelectedItem != null)
            {
                PersonDetails.castinfo = (CastInfo)this.lbCast.SelectedItem;

                this.lbCast.SelectedIndex = -1;

                NavigationService.Navigate(new Uri("/PersonDetails.xaml", UriKind.Relative));
            }
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

        CastInfo ci = null;

        private async void Cast_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid g = (sender as Grid);

            CastInfo newCI = (CastInfo)g.Tag;

            if (Config.AudioSupport && ci != newCI)
            {
                ci = newCI;

                await SpeakCastInfo(newCI);

                return;
            }
            else
            {
                PersonDetails.castinfo = newCI;

                NavigationService.Navigate(new Uri("/PersonDetails.xaml", UriKind.Relative));
            }
        }

        private async Task SpeakCastInfo(CastInfo ci)
        {
            await SpeechSynthesisService.SpeakOutLoud(ci.Title);
        }

        private async void FilmPoster_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            await SpeakFilmInfo(SelectedFilm);
        }

        private async Task SpeakFilmInfo(FilmInfo fi)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fi.Title);
            sb.AppendFormat("classification {0}\n", fi.Classification);

            if (fi.VoteCount > 0)
                sb.AppendFormat("average rating {0}, {1} reviews\n", fi.AverageRating.ToString("N2"), fi.VoteCount);

            sb.AppendFormat("release date {0}\n", fi.Release.ToLongDateString());

            if (fi.Runtime > 0)
                sb.AppendFormat("duration {0} minutes\n", fi.Runtime);

            sb.AppendFormat("overview {0}", fi.Overview);

            await SpeechSynthesisService.SpeakOutLoud(sb.ToString());
        }

        private async void pMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Config.AudioSupport)
                return;

            string speakOut = null;
            switch (this.pMain.SelectedIndex)
            {
                case 0:
                    speakOut = String.Format("overview of {0}", SelectedFilm.Title);
                    break;

                case 1:
                    speakOut = "film cast";
                    break;

                case 2:
                    speakOut = "user reviews";
                    break;

                default:
                    speakOut = "showing at the cinemas below";
                    break;
            }

            await SpeechSynthesisService.SpeakOutLoud(speakOut);
        }

        CinemaInfo cinema = null;
        private async void Cinema_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TextBlock tb = (sender as TextBlock);
            CinemaInfo newCinema = (tb.Tag as CinemaInfo);
            if (Config.AudioSupport && cinema != newCinema)
            {
                cinema = newCinema;

                await SpeechSynthesisService.SpeakOutLoud(newCinema.Name);

                return;
            }
            else
            {
                ShowPerformances.SelectedCinema = newCinema;
                ShowPerformances.SelectedFilm = SelectedFilm;
                ShowPerformances.ShowCinemaDetails = true;
                ShowPerformances.ShowFilmDetails = false;
                NavigationService.Navigate(new Uri("/ShowPerformances.xaml", UriKind.Relative));
            }
        }
    }
}