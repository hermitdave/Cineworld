using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using PDRatingSample;
using System.Drawing;

namespace CineworldiPhone
{
	partial class FilmDetailsController : UIViewController
	{
		public FilmDetailsController (IntPtr handle) : base (handle)
		{
		}

		public FilmInfo Film { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			string url = this.Film.PosterUrl == null ? null : this.Film.PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			this.Poster.Image = image;

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

			/*
			float height = 0;
			if (!String.IsNullOrWhiteSpace (this.Film.Tagline)) 
			{
				this.TaglineData.Text = this.Film.Tagline;
			} else 
			{
				this.TaglineData.Hidden = this.TaglineLabel.Hidden = true;
			}
*/
			if (!String.IsNullOrWhiteSpace (this.Film.Overview)) 
			{
				this.OverviewData.Text = this.Film.Overview;
			}
			else
			{
				this.OverviewData.Hidden = this.OverviewLabel.Hidden = true;
			}

			var bounds = this.View.Bounds;
			this.FilmCastTable.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			this.FilmCastTable.Source = new FilmCastTableSource (this.Film.FilmCast);
			this.FilmCastTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.FilmCastTable.Hidden = true;


			UITableView cinemasTable = new UITableView(new RectangleF(0, 95, (float)bounds.Width, (float)bounds.Height-95));
			cinemasTable.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			cinemasTable.Source = new CinemasTableSource (Application.FilmCinemas[this.Film.EDI]);
			cinemasTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			cinemasTable.Hidden = true;
			this.View.AddSubview (cinemasTable);


			UIButton rateReview = new UIButton (UIButtonType.RoundedRect);
			rateReview.Frame = new RectangleF (0, 95, (float)bounds.Width, 30);
			rateReview.SetTitle ("Rate & Review", UIControlState.Normal);
			rateReview.Hidden = true;
			this.View.AddSubview (rateReview);

			UITableView reviewstable = new UITableView(new RectangleF(0, 125, (float)bounds.Width, (float)bounds.Height-125));
			reviewstable.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			reviewstable.Source = new ReviewsTableSource (this.Film.Reviews);
			reviewstable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			reviewstable.Hidden = true;
			this.View.AddSubview (reviewstable);


			this.FilmDetailSegments.ValueChanged += (sender, e) => 
			{
				this.Poster.Hidden = this.Misc.Hidden = this.OverviewLabel.Hidden = this.OverviewData.Hidden = true;

				this.FilmCastTable.Hidden = true;

				rateReview.Hidden = reviewstable.Hidden = true;

				cinemasTable.Hidden = true;

				switch(this.FilmDetailSegments.SelectedSegment)
				{
					case 0:
					this.Poster.Hidden = false;
					this.Misc.Hidden = false;
					this.OverviewLabel.Hidden = this.OverviewData.Hidden = false;
					break;

					case 1:
					this.FilmCastTable.Hidden = false;
					break;

					case 2:
					rateReview.Hidden = reviewstable.Hidden = false;
					break;

					case 3:
					cinemasTable.Hidden = false;
					break;
				}
			};
		}
	}
}
