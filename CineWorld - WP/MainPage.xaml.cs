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
using System.Collections.ObjectModel;

namespace CineWorld
{
    public partial class MainPage : PhoneApplicationPage
    {
        CineWorldService cws = new CineWorldService();
        private LongListSelector currentSelector;

        static bool bLoaded = false;
        //List<string> cinemas = new List<string>();
        //List<string> films = new List<string>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //this.cinemas.Add("Aberdeen-Union-Square");
            //this.cinemas.Add("Aberdeen-Union-Street");

            //this.films.Add("Avengers");
            //this.films.Add("Men In Black 3");

            // Set the data context of the listbox control to the sample data
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.llsCinemas.SelectedItem != null)
                this.llsCinemas.SelectedItem = null;

            if (this.llsFilms.SelectedItem != null)
                this.llsFilms.SelectedItem = null;

            if (bLoaded)
                return;

            this.scWaiting.IsSpinning = true;

            cws.GetCinemasAsyncComplete += new Action<CinemasEventArgs>(cws_GetCinemasAsyncComplete);
            cws.GetFilmsAsyncComplete += new Action<FilmsEventArgs>(cws_GetFilmsAsyncComplete);
            
            cws.GetFilmsAsync(true);
            cws.GetCinemasAsync(true);

            bLoaded = true;

            base.OnNavigatedTo(e);
        }

        void cws_GetFilmsAsyncComplete(FilmsEventArgs obj)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    if (String.IsNullOrEmpty(obj.ErrorMessage))
                    {
                        this.llsFilms.SelectionChanged -= new SelectionChangedEventHandler(lbFilms_SelectionChanged);

                        this.llsFilms.ItemsSource = obj.Films.films;

                        this.llsFilms.SelectionChanged += new SelectionChangedEventHandler(lbFilms_SelectionChanged);
                    }
                    else
                        MessageBox.Show(obj.ErrorMessage, "Films", MessageBoxButton.OK);

                    this.scWaiting.IsSpinning = false;
                });
        }

        private void LongListSelector_GroupViewOpened(object sender, GroupViewOpenedEventArgs e)
        {
            //Hold a reference to the active long list selector.
            currentSelector = sender as LongListSelector;

            //Construct and begin a swivel animation to pop in the group view.
            IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            Storyboard _swivelShow = new Storyboard();
            ItemsControl groupItems = e.ItemsControl;

            foreach (var item in groupItems.Items)
            {
                UIElement container = groupItems.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (container != null)
                {
                    Border content = VisualTreeHelper.GetChild(container, 0) as Border;
                    if (content != null)
                    {
                        DoubleAnimationUsingKeyFrames showAnimation = new DoubleAnimationUsingKeyFrames();

                        EasingDoubleKeyFrame showKeyFrame1 = new EasingDoubleKeyFrame();
                        showKeyFrame1.KeyTime = TimeSpan.FromMilliseconds(0);
                        showKeyFrame1.Value = -60;
                        showKeyFrame1.EasingFunction = quadraticEase;

                        EasingDoubleKeyFrame showKeyFrame2 = new EasingDoubleKeyFrame();
                        showKeyFrame2.KeyTime = TimeSpan.FromMilliseconds(85);
                        showKeyFrame2.Value = 0;
                        showKeyFrame2.EasingFunction = quadraticEase;

                        showAnimation.KeyFrames.Add(showKeyFrame1);
                        showAnimation.KeyFrames.Add(showKeyFrame2);

                        Storyboard.SetTargetProperty(showAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
                        Storyboard.SetTarget(showAnimation, content.Projection);

                        _swivelShow.Children.Add(showAnimation);
                    }
                }
            }

            _swivelShow.Begin();
        }

        private void LongListSelector_GroupViewClosing(object sender, GroupViewClosingEventArgs e)
        {
            //Cancelling automatic closing and scrolling to do it manually.
            e.Cancel = true;
            if (e.SelectedGroup != null)
            {
                currentSelector.ScrollToGroup(e.SelectedGroup);
            }

            //Dispatch the swivel animation for performance on the UI thread.
            Dispatcher.BeginInvoke(() =>
            {
                //Construct and begin a swivel animation to pop out the group view.
                IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                Storyboard _swivelHide = new Storyboard();
                ItemsControl groupItems = e.ItemsControl;

                foreach (var item in groupItems.Items)
                {
                    UIElement container = groupItems.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                    if (container != null)
                    {
                        Border content = VisualTreeHelper.GetChild(container, 0) as Border;
                        if (content != null)
                        {
                            DoubleAnimationUsingKeyFrames showAnimation = new DoubleAnimationUsingKeyFrames();

                            EasingDoubleKeyFrame showKeyFrame1 = new EasingDoubleKeyFrame();
                            showKeyFrame1.KeyTime = TimeSpan.FromMilliseconds(0);
                            showKeyFrame1.Value = 0;
                            showKeyFrame1.EasingFunction = quadraticEase;

                            EasingDoubleKeyFrame showKeyFrame2 = new EasingDoubleKeyFrame();
                            showKeyFrame2.KeyTime = TimeSpan.FromMilliseconds(125);
                            showKeyFrame2.Value = 90;
                            showKeyFrame2.EasingFunction = quadraticEase;

                            showAnimation.KeyFrames.Add(showKeyFrame1);
                            showAnimation.KeyFrames.Add(showKeyFrame2);

                            Storyboard.SetTargetProperty(showAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
                            Storyboard.SetTarget(showAnimation, content.Projection);

                            _swivelHide.Children.Add(showAnimation);
                        }
                    }
                }

                _swivelHide.Completed += _swivelHide_Completed;
                _swivelHide.Begin();

            });
        }

        private void _swivelHide_Completed(object sender, EventArgs e)
        {
            //Close group view.
            if (currentSelector != null)
            {
                currentSelector.CloseGroupView();
                currentSelector = null;
            }
        }


        void cws_GetCinemasAsyncComplete(CinemasEventArgs obj)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    if (String.IsNullOrEmpty(obj.ErrorMessage))
                    {
                        this.llsCinemas.SelectionChanged -= new SelectionChangedEventHandler(lbCinemas_SelectionChanged);
                        var cinemasByChar = from cinema in obj.Cinemas.cinemas
                                            group cinema by cinema.Region into c
                                            orderby c.Key
                                            select new PublicGrouping<string, Cinema>(c);


                        this.llsCinemas.ItemsSource = cinemasByChar;

                        this.llsCinemas.SelectionChanged += new SelectionChangedEventHandler(lbCinemas_SelectionChanged);
                    }
                    else
                        MessageBox.Show(obj.ErrorMessage, "Cimemas", MessageBoxButton.OK);
                });
        }

        void lbFilms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.llsFilms.SelectedItem != null)
            {
                FilmDetails.SelectedFilm = (Film)this.llsFilms.SelectedItem;
                this.llsFilms.SelectionChanged -= new SelectionChangedEventHandler(lbFilms_SelectionChanged);
                this.llsFilms.SelectedItem = null;
                this.llsFilms.SelectionChanged += new SelectionChangedEventHandler(lbFilms_SelectionChanged);
                NavigationService.Navigate(new Uri("/FilmDetails.xaml", UriKind.Relative));
            }
        }

        void lbCinemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.llsCinemas.SelectedItem != null)
            {
                CinemaDetails.SelectedCinema = (Cinema)this.llsCinemas.SelectedItem;
                this.llsCinemas.SelectionChanged -= new SelectionChangedEventHandler(lbCinemas_SelectionChanged);
                this.llsCinemas.SelectedItem = null;
                this.llsCinemas.SelectionChanged += new SelectionChangedEventHandler(lbCinemas_SelectionChanged);
                NavigationService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
            }
        }

        //private void appbar_info_Click(object sender, EventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        //}

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        //private void mainAppBar_StateChanged(object sender, Microsoft.Phone.Shell.ApplicationBarStateChangedEventArgs e)
        //{
        //    this.ApplicationBar.Opacity = (e.IsMenuVisible ? 1 : 0);
        //}
    }
}