using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class Review : Page
    {
        public enum ReviewTargetDef
        {
            Film,
            Cinema,
        }

        public static FilmInfo SelectedFilm { get; set; }
        public static CinemaInfo SelectedCinema { get; set; }
        public static ReviewTargetDef ReviewTarget { get; set; }

        ReviewBase UserReview { get; set; }

        public Review()
        {
            this.InitializeComponent();

            if (App.MobileService == null)
                App.MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void tbReview_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.tbReview.Text))
                this.tbCharCount.Text = "140";
            else
                this.tbCharCount.Text = String.Format("{0} / 140", 140 - this.tbReview.Text.Length);
        }

        private void PopulateExistingReview()
        {
            if (!String.IsNullOrWhiteSpace(this.UserReview.Reviewer))
                this.tbName.Text = this.UserReview.Reviewer.Trim();

            //this.rating.Value = this.UserReview.Rating;

            if (!String.IsNullOrWhiteSpace(this.UserReview.Review))
                this.tbReview.Text = this.UserReview.Review.Trim();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.ContentPanel.Opacity = bNewVal ? 0.5 : 1;
            this.ContentPanel.IsHitTestVisible = !bNewVal;

            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //if (this.rating.Value == 0)
            //{
            //    await new MessageDialog(String.Format("please rate this {0}", ReviewTarget.ToString())).ShowAsync();
            //    return;
            //}

            this.SpinAndWait(true);

            this.UserReview.Reviewer = String.IsNullOrWhiteSpace(this.tbName.Text) ? "anonymous" : this.tbName.Text;
            this.UserReview.Review = this.tbReview.Text.Trim();
            //this.UserReview.Rating = Convert.ToInt16(this.rating.Value);
            this.UserReview.ReviewTS = DateTime.Now;
            this.UserReview.UserId = ExtendedPropertyHelper.GetUserIdentifier();

            Config.UserName = this.UserReview.Reviewer;

            if (ReviewTarget == ReviewTargetDef.Film)
            {
                FilmReview fr = (FilmReview)this.UserReview;
                fr.Movie = SelectedFilm.EDI;

                if (this.UserReview.Id != 0)
                {
                    await App.MobileService.GetTable<FilmReview>().UpdateAsync(fr);
                }
                else
                {
                    await App.MobileService.GetTable<FilmReview>().InsertAsync(fr);
                }
            }
            else
            {
                CinemaReview cr = (CinemaReview)this.UserReview;
                cr.Cinema = SelectedCinema.ID;

                if (this.UserReview.Id != 0)
                {
                    await App.MobileService.GetTable<CinemaReview>().UpdateAsync(cr);
                }
                else
                {
                    await App.MobileService.GetTable<CinemaReview>().InsertAsync(cr);
                }
            }

            this.SpinAndWait(false);

            (((this.Parent as FrameworkElement).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.SpinAndWait(false);

            (((this.Parent as FrameworkElement).Parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ReviewTarget == ReviewTargetDef.Film)
            {
                this.tbReviewing.Text = SelectedFilm.Title;

                List<FilmReview> filmreviews = await App.MobileService.GetTable<FilmReview>().Where(r => r.Movie == SelectedFilm.EDI && r.UserId == ExtendedPropertyHelper.GetUserIdentifier()).ToListAsync();
                if (filmreviews != null && filmreviews.Count > 0)
                    this.UserReview = filmreviews[0];
                else
                    this.UserReview = new FilmReview() { Reviewer = Config.UserName };
            }
            else
            {
                this.tbReviewing.Text = String.Format("Cineworld - {0}", SelectedCinema.Name);

                List<CinemaReview> cinemareviews = await App.MobileService.GetTable<CinemaReview>().Where(r => r.Cinema == SelectedCinema.ID && r.UserId == ExtendedPropertyHelper.GetUserIdentifier()).ToListAsync();
                if (cinemareviews != null && cinemareviews.Count > 0)
                    this.UserReview = cinemareviews[0];
                else
                    this.UserReview = new CinemaReview() { Reviewer = Config.UserName };
            }

            this.PopulateExistingReview();
        }
    }
}
