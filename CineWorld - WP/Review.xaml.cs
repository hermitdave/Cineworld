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
using Microsoft.WindowsAzure.MobileServices;

namespace CineWorld
{
    public partial class Review : PhoneApplicationPage
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
            InitializeComponent();

            if (App.MobileService == null)
                App.MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");
        }

        private void tbReview_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.tbReview.Text))
                this.tbCharCount.Text = "140";
            else
                this.tbCharCount.Text = String.Format("{0} / 140", 140 - this.tbReview.Text.Length);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.SpinAndWait(false);

            if (ReviewTarget == ReviewTargetDef.Film)
            {
                this.tbReviewing.Text = SelectedFilm.Title;
                List<FilmReview> filmreviews = null;

                try
                {
                    filmreviews = await App.MobileService.GetTable<FilmReview>().Where(r => r.Movie == SelectedFilm.EDI && r.UserId == ExtendedPropertyHelper.GetUserIdentifier()).ToListAsync();
                }
                catch { }
                
                if (filmreviews != null && filmreviews.Count > 0)
                    this.UserReview = filmreviews[0];
                else
                    this.UserReview = new FilmReview() { Reviewer = Config.UserName };
            }
            else
            {
                this.tbReviewing.Text = String.Format("Cineworld - {0}", SelectedCinema.Name);

                List<CinemaReview> cinemareviews = null;

                try
                {
                    cinemareviews = await App.MobileService.GetTable<CinemaReview>().Where(r => r.Cinema == SelectedCinema.ID && r.UserId == ExtendedPropertyHelper.GetUserIdentifier()).ToListAsync();
                }
                catch { }
                if (cinemareviews != null && cinemareviews.Count > 0)
                    this.UserReview = cinemareviews[0];
                else
                    this.UserReview = new CinemaReview() { Reviewer = Config.UserName };
            }

            this.PopulateExistingReview();
        }

        private void PopulateExistingReview()
        {
            if (!String.IsNullOrWhiteSpace(this.UserReview.Reviewer))
                this.tbName.Text = this.UserReview.Reviewer.Trim();

            this.rating.Value = this.UserReview.Rating;

            if (!String.IsNullOrWhiteSpace(this.UserReview.Review))
                this.tbReview.Text = this.UserReview.Review.Trim();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.ContentPanel.Opacity = bNewVal ? 0.5 : 1;
            this.ContentPanel.IsHitTestVisible = !bNewVal;
            this.scWaiting.Visibility = bNewVal ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            this.scWaiting.IsSpinning = bNewVal;
        }

        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            if (this.rating.Value == 0)
            {
                MessageBox.Show(String.Format("please rate this {0}", ReviewTarget.ToString()));
                return;
            }

            this.SpinAndWait(true);

            this.UserReview.Reviewer = String.IsNullOrWhiteSpace(this.tbName.Text) ? "anonymous" : this.tbName.Text;
            this.UserReview.Review = this.tbReview.Text.Trim();
            this.UserReview.Rating = Convert.ToInt16(this.rating.Value);
            this.UserReview.ReviewTS = DateTime.Now;
            this.UserReview.UserId = ExtendedPropertyHelper.GetUserIdentifier();

            Config.UserName = this.UserReview.Reviewer;

            try
            {
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
            }
            catch
            {
                MessageBox.Show("error saving review. please try again later");
            }

            this.SpinAndWait(false);

            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}