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

		public const string YouTubeEmbedUrl = "https://www.youtube.com/embed/{0}";
		public const string YouTubeEmbedString = "<html><body><iframe width=\"290\" height=\"163\" src=\"{0}\" frameborder=\"0\" allowfullscreen></iframe></body></html>";

		public FilmInfo Film { get; set; }

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			PerformancesController performancesController = (segue.DestinationViewController as PerformancesController);

			if (performancesController != null) 
			{
				performancesController.Showing = PerformancesController.ViewType.CinemaDetails;
				performancesController.Cinema = (sender as CinemaTableCell).Cinema;
				performancesController.Film = this.Film;
			} 
			else 
			{
				ReviewController reviewController = (segue.DestinationViewController as ReviewController);

				if (reviewController != null) 
				{
					reviewController.FilmDetailsController = this;
					reviewController.Film = this.Film;
				} 
				else
				{
					YouTubeController youtubeController = segue.DestinationViewController as YouTubeController;
					youtubeController.YouTubeId = this.Film.YoutubeTrailer;
				}
			}
		}

		public void ReviewSubmitted()
		{
			this.NavigationController.PopViewController(true);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.FilmTitle.Title = this.Film.TitleWithClassification;

			this.Poster.Hidden = false;

			this.Poster.Layer.CornerRadius = 10f;
			this.Poster.Layer.MasksToBounds = true;
			this.Poster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Poster.Layer.Opaque = true;

			if (!String.IsNullOrWhiteSpace (this.Film.YoutubeTrailer)) 
			{
				this.PlayTrailer.Hidden = false;
				//this.PlayTrailer.TouchUpInside += (sender, e) => 
				//{
					// open youtube url
					//UIApplication.SharedApplication.OpenUrl(new NSUrl(String.Format(@"http://www.youtube.com/watch?v={0}", this.Film.YoutubeTrailer)));
				//};
			} 

			string url = this.Film.PosterUrl == null ? null : this.Film.PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			this.Poster.Image = ImageHelper.ResizeImage(image, 109, 163);

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(0f, 0f, 60f , 25f), ratingConfig, Convert.ToDecimal(this.Film.AverageRating));

			this.Misc.AddSubview (ratingView);

			var reviewCount = new UILabel (new RectangleF (70f, 0f, (float)(this.Misc.Bounds.Width-60), 25f));
			reviewCount.Font = UIFont.FromName ("HelveticaNeue", 12f);
			reviewCount.Lines = 1;
			reviewCount.Text = String.Format ("{0} ratings", this.Film.Reviews.Count);
			this.Misc.AddSubview (reviewCount);

			var durationLabel = new UILabel (new RectangleF (0f, 25f, (float)this.Misc.Bounds.Width, 25f));
			durationLabel.Font = UIFont.FromName ("HelveticaNeue", 12f);
			durationLabel.Lines = 0;
			durationLabel.Text = String.Format ("Duration {0} mins", this.Film.Runtime);
			this.Misc.AddSubview (durationLabel);

			var releaseLabel = new UILabel (new RectangleF (0f, 50f, (float)this.Misc.Bounds.Width, 25f));
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
				this.OverviewData.SizeToFit ();

				this.FilmGistView.SizeToFit ();
				//var height = OverviewData.Bounds.Top + OverviewData.Bounds.Height + 15;
				//this.GistScrollViewer.ContentSize = new CoreGraphics.CGSize (320, height);
				//this.GistScrollViewer.
			}
			else
			{
				this.OverviewData.Hidden = this.OverviewLabel.Hidden = true;
			}

			var bounds = this.View.Bounds;
			//this.FilmCastTable.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			this.FilmCastTable.Source = new FilmCastTableSource (this.Film.FilmCast);
			//this.FilmCastTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.FilmCastTable.Hidden = true;


			//UITableView cinemasTable = new UITableView(new RectangleF(10, 123, (float)bounds.Width-10, (float)bounds.Height-123));
			//cinemasTable.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			this.CinemasView.Source = new CinemasTableSource (Application.FilmCinemas[this.Film.EDI]);
			//cinemasTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.CinemasView.Hidden = true;
			//this.View.AddSubview (cinemasTable);


			this.ReviewTable.Source = new ReviewsTableSource (this.Film.Reviews);


			this.FilmDetailSegments.ValueChanged += (sender, e) => 
			{
				this.GistScrollViewer.Hidden = true;

				this.FilmCastTable.Hidden = true;

				this.ReviewsView.Hidden = true;

				this.CinemasView.Hidden = true;

				switch(this.FilmDetailSegments.SelectedSegment)
				{
					case 0:
					this.GistScrollViewer.Hidden = false;
					break;

					case 1:
					this.FilmCastTable.Hidden = false;
					break;

					case 2:
					this.ReviewsView.Hidden = false;
					break;

					case 3:
					this.CinemasView.Hidden = false;
					break;
				}
			};
		}
	}
}
