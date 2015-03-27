using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using PDRatingSample;
using System.Drawing;
using Cineworld;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Query;
using System.Collections.Generic;

namespace CineworldiPhone
{
	partial class ReviewController : UIViewController
	{
		public const string UserReviewDefault = "Your review in 140 characters!";
		PDRatingView RatingView;

		public FilmInfo Film { get; set; }
		public FilmDetailsController FilmDetailsController { get; set; }

		public CinemaInfo Cinema { get; set; }
		public CinemaDetailsController CinemaDetailsController { get; set; }

		public PerformancesController PerformancesController { get; set; }

		ReviewBase UserReview;

		public ReviewController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			this.RatingView = new PDRatingView(new RectangleF(90f, 120f, 215f, 35f), ratingConfig, Convert.ToDecimal(0));

			this.RatingView.RatingChosen += (sender, e) => {
				this.UserReview.Rating = Convert.ToInt16(e.Rating);
			};

			// [Required] Add the view to the 
			this.View.AddSubview(this.RatingView);

			this.Review.Text = UserReviewDefault;
			this.Review.TextColor = UIColor.LightGray;

			this.Review.Layer.BorderColor = UIColor.LightGray.CGColor;
			this.Review.Layer.BorderWidth = 0.5f;
			this.Review.BackgroundColor = UIColor.White;
			this.Review.Layer.CornerRadius = 10f;
			this.Review.Layer.MasksToBounds = true;
			this.Review.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Review.Layer.Opaque = true;

			this.Review.ShouldBeginEditing = t => {
				if (this.Review.Text == UserReviewDefault) {
					this.Review.Text = string.Empty;
					this.Review.TextColor = UIColor.Black;
				}

				return true;
			};

			this.Review.ShouldEndEditing = t => {
				if (string.IsNullOrEmpty (this.Review.Text)) 
				{
					this.Review.Text = UserReviewDefault;
					this.Review.TextColor = UIColor.LightGray;
				}
				return true;
			};

			this.Review.ShouldChangeText += delegate 
			{
				if (this.Review.Text.Length < 140) 
				{
					return true;
				}

				return false;
			};



			if (Application.MobileService == null) 
			{
				CurrentPlatform.Init ();
				Application.MobileService = new MobileServiceClient ("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");
			}

			this.Submit.TouchUpInside += Submit_TouchUpInside;

			if (this.Film != null) 
			{
				this.NavigationItem.Title = this.Film.Title;

				List<FilmReview> filmreviews = null;

				try
				{
					filmreviews = await Application.MobileService.GetTable<FilmReview>().Where(r => r.Movie == this.Film.EDI && r.UserId == Config.UserGuid).ToListAsync();
				}
				catch { }


				if (filmreviews != null && filmreviews.Count > 0)
					this.UserReview = filmreviews[0];
				else
					this.UserReview = new FilmReview() { Reviewer = Config.UserName };
			} 
			else if(this.Cinema != null)
			{
				this.NavigationItem.Title = this.Cinema.Name;

				List<CinemaReview> cinemareviews = null;

				try
				{
					cinemareviews = await Application.MobileService.GetTable<CinemaReview>().Where(r => r.Cinema == this.Cinema.ID && r.UserId == Config.UserGuid).ToListAsync();
				}
				catch { }

				if (cinemareviews != null && cinemareviews.Count > 0)
					this.UserReview = cinemareviews[0];
				else
					this.UserReview = new CinemaReview() { Reviewer = Config.UserName };
			}

			if (!String.IsNullOrWhiteSpace(this.UserReview.Reviewer))
				this.User.Text = this.UserReview.Reviewer.Trim();

			this.RatingView.AverageRating = this.UserReview.Rating;

			if (!String.IsNullOrWhiteSpace (this.UserReview.Review)) 
			{
				this.Review.Text = this.UserReview.Review.Trim ();
				this.Review.TextColor = UIColor.Black;
			}
		}

		async void Submit_TouchUpInside (object sender, EventArgs e)
		{
			this.UserReview.Reviewer = String.IsNullOrWhiteSpace(this.User.Text) ? "anonymous" : this.User.Text;
			this.UserReview.Review = this.Review.Text.Trim();
			this.UserReview.ReviewTS = DateTime.Now;
			this.UserReview.UserId = Config.UserGuid;

			Config.UserName = this.UserReview.Reviewer;

			try
			{
				this.BusyIndicator.StartAnimating();

				if (this.Film != null)
				{
					FilmReview fr = (FilmReview)this.UserReview;
					fr.Movie = this.Film.EDI;
					fr.TmdbId = this.Film.TmdbId;

					if (this.UserReview.Id != 0)
					{
						await Application.MobileService.GetTable<FilmReview>().UpdateAsync(fr);
					}
					else
					{
						await Application.MobileService.GetTable<FilmReview>().InsertAsync(fr);
					}
				}
				else
				{
					CinemaReview cr = (CinemaReview)this.UserReview;
					cr.Cinema = this.Cinema.ID;

					if (this.UserReview.Id != 0)
					{
						await Application.MobileService.GetTable<CinemaReview>().UpdateAsync(cr);
					}
					else
					{
						await Application.MobileService.GetTable<CinemaReview>().InsertAsync(cr);
					}
				}

				if(this.FilmDetailsController != null)
				{
					this.FilmDetailsController.ReviewSubmitted();
				}
				else if(this.CinemaDetailsController != null)
				{
					this.CinemaDetailsController.ReviewSubmitted();
				}
				else if(this.PerformancesController != null)
				{
					this.PerformancesController.ReviewSubmitted();
				}
			}
			catch 
			{
				UIAlertView alert = new UIAlertView ("Cineworld", "Error saving review. Please try again later", null, "OK", null);
				alert.Show();
			}
			finally 
			{
				this.BusyIndicator.StopAnimating ();
			}
		}
	}
}
