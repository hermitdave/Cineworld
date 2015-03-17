using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Linq;
using System.Threading.Tasks;
using Cineworld;
using PDRatingSample;
using System.Drawing;

namespace CineworldiPhone
{
	partial class PerformancesController : UIViewController
	{
		public enum ViewType
		{
			FilmDetails,
			CinemaDetails
		}

		public ViewType Showing { get; set; }

		public CinemaInfo Cinema { get; set; }

		public FilmInfo Film { get; set; }

		public PerformancesController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.FilmGist.Hidden = this.FilmCast.Hidden = 
				this.FilmReviews.Hidden = this.CinemaGist.Hidden =
					this.PerformanceView.Hidden = true;
			

			if (this.Showing == ViewType.CinemaDetails) 
			{
				this.PerformancesSegment.RemoveSegmentAtIndex (2, false);
				this.PerformancesSegment.RemoveSegmentAtIndex (1, false);
				this.PerformancesSegment.RemoveSegmentAtIndex (0, false);

				this.LoadCinemaDetails ();

				this.CinemaGist.Hidden = false;
			} 
			else 
			{
				this.PerformancesSegment.RemoveSegmentAtIndex (3, false);

				this.LoadFilmDetails ();

				this.FilmGist.Hidden = false;
			}

			await this.LoadPerformances();

			this.PerformancesSegment.ValueChanged += (sender, e) => 
			{
				this.FilmGist.Hidden = this.FilmCast.Hidden = 
					this.FilmReviews.Hidden = this.CinemaGist.Hidden =
						this.PerformanceView.Hidden = true;

				if(this.Showing == ViewType.CinemaDetails)
				{
					switch(this.PerformancesSegment.SelectedSegment)
					{
					case 0:
						this.CinemaGist.Hidden = false;
						break;

					case 1:
						this.PerformanceView.Hidden = false;
						break;
					}
				}
				else
				{
					switch(this.PerformancesSegment.SelectedSegment)
					{
					case 0:
						this.FilmGist.Hidden = false;
						break;

					case 1:
						this.FilmCast.Hidden = false;
						break;

					case 2:
						this.FilmReviews.Hidden = false;
						break;

					case 3:
						this.PerformanceView.Hidden = false;
						break;
					}
				}
			};
		}

		void LoadFilmDetails ()
		{
			string url = this.Film.PosterUrl == null ? null : this.Film.PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			this.Poster.Image = image.Scale( new CoreGraphics.CGSize(130, 196));

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(0f, 0f, 60f , 30f), ratingConfig, Convert.ToDecimal(this.Film.AverageRating));

			this.Misc.AddSubview (ratingView);

			var reviewCount = new UILabel (new RectangleF (70f, 0f, (float)(this.Misc.Bounds.Width-60), 30f));
			reviewCount.Font = UIFont.FromName ("HelveticaNeue", 12f);
			reviewCount.Lines = 1;
			reviewCount.Text = String.Format ("{0} ratings", this.Film.Reviews.Count);
			this.Misc.AddSubview (reviewCount);

			var durationLabel = new UILabel (new RectangleF (0f, 30f, (float)this.Misc.Bounds.Width, 30f));
			durationLabel.Font = UIFont.FromName ("HelveticaNeue", 12f);
			durationLabel.Lines = 0;
			durationLabel.Text = String.Format ("Duration {0} mins", this.Film.Runtime);
			this.Misc.AddSubview (durationLabel);

			var releaseLabel = new UILabel (new RectangleF (0f, 60f, (float)this.Misc.Bounds.Width, 30f));
			releaseLabel.Font = UIFont.FromName ("HelveticaNeue", 12f);
			releaseLabel.Lines = 0;
			releaseLabel.Text = String.Format ("Release {0}", this.Film.ReleaseDate);
			this.Misc.AddSubview (releaseLabel);

			if (!String.IsNullOrWhiteSpace (this.Film.Overview)) 
			{
				this.OverviewData.Text = this.Film.Overview;
				this.OverviewData.SizeToFit ();
			}
			else
			{
				this.OverviewData.Hidden = this.OverviewLabel.Hidden = true;
			}

			this.FilmCast.Source = new FilmCastTableSource (this.Film.FilmCast);

			this.FilmReviewTable.Source = new ReviewsTableSource (this.Film.Reviews);

			this.BusyIndicator.Hidden = true;
		}

		void LoadCinemaDetails ()
		{
			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(20f, 0f, 50f, 30f), ratingConfig, Convert.ToDecimal(this.Cinema.AverageRating));

			// [Required] Add the view to the 
			this.CinemaGist.AddSubview(ratingView);

			var reviewCount = new UILabel (new RectangleF (80f, 0f, 60f, 30f));
			reviewCount.Font = UIFont.FromName ("HelveticaNeue", 12f);
			reviewCount.Text = String.Format ("{0} ratings", this.Cinema.Reviews.Count);
			this.CinemaGist.AddSubview (reviewCount);

			this.Address.Text = this.Cinema.FullAddress;

			this.Telephone.Text = this.Cinema.Telephone;

			this.CinemaReviewsTable.Source = new ReviewsTableSource (this.Cinema.Reviews);
			this.CinemaReviewsTable.ReloadData ();
		}

		async Task LoadPerformances ()
		{
			if (this.Film.Performances == null || this.Film.Performances.Count == 0) 
			{
				this.BusyIndicator.StartAnimating ();
				this.BusyIndicator.Hidden = false;

				await new LocalStorageHelper ().GetCinemaFilmListings (this.Cinema.ID, false);

				this.Film = Application.CinemaFilms[this.Cinema.ID].First(f => f.EDI == this.Film.EDI);

				this.BusyIndicator.StopAnimating ();
				this.BusyIndicator.Hidden = true;
			}
			this.PerformanceView.Source = new PerformancesTableSource (this.Film.Performances);
			this.PerformanceView.ReloadData ();
		}
	}
}
