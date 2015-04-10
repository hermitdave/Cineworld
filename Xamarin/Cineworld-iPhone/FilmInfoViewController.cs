using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using PDRatingSample;
using System.Drawing;

namespace CineworldiPhone
{
	partial class FilmInfoViewController : UIViewController
	{
		public FilmInfo Film { get; set; }

		public FilmInfoViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.imgPoster.Layer.CornerRadius = 10f;
			this.imgPoster.Layer.MasksToBounds = true;
			this.imgPoster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.imgPoster.Layer.Opaque = true;

			if (!String.IsNullOrWhiteSpace (this.Film.YoutubeTrailer)) 
			{
				this.btnTrailer.Hidden = false;

				this.btnTrailer.Layer.CornerRadius = 10f;
				this.btnTrailer.Layer.MasksToBounds = true;
				this.btnTrailer.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
				this.btnTrailer.Layer.Opaque = true;
			} 

			string url = this.Film.PosterUrl == null ? null : this.Film.PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			this.imgPoster.Image = ImageHelper.ResizeImage(image, 109, 163);

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(0f, 0f, 60f , 25f), ratingConfig, Convert.ToDecimal(this.Film.AverageRating));

			this.viewMisc.AddSubview (ratingView);

			var reviewCount = new UILabel (new RectangleF (70f, 0f, (float)(this.viewMisc.Bounds.Width-60), 25f));
			reviewCount.Font = UIFont.FromName ("HelveticaNeue", 12f);
			reviewCount.Lines = 1;
			reviewCount.Text = String.Format ("{0} ratings", this.Film.Reviews.Count);
			this.viewMisc.AddSubview (reviewCount);

			var durationLabel = new UILabel (new RectangleF (0f, 25f, (float)this.viewMisc.Bounds.Width, 25f));
			durationLabel.Font = UIFont.FromName ("HelveticaNeue", 12f);
			durationLabel.Lines = 0;
			durationLabel.Text = String.Format ("Duration {0} mins", this.Film.Runtime);
			this.viewMisc.AddSubview (durationLabel);

			var releaseLabel = new UILabel (new RectangleF (0f, 50f, (float)this.viewMisc.Bounds.Width, 25f));
			releaseLabel.Font = UIFont.FromName ("HelveticaNeue", 12f);
			releaseLabel.Lines = 0;
			releaseLabel.Text = String.Format ("Release {0}", this.Film.ReleaseDate);
			this.viewMisc.AddSubview (releaseLabel);

			if (!String.IsNullOrWhiteSpace (this.Film.Overview)) 
			{
				this.lblOverview.Text = this.Film.Overview;
				this.lblOverview.SizeToFit ();
			}
			else
			{
				this.lblOverview.Hidden = this.lblOverviewHeader.Hidden = true;
			}
		}
	}
}
