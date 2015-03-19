using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using PDRatingSample;
using System.Drawing;

namespace CineworldiPhone
{
	partial class FilmTableCell : UITableViewCell
	{
		public FilmInfo Film { get; private set; }

		public FilmTableCell(IntPtr handle) : base(handle)
		{
		}

		public void UpdateCell(FilmInfo film, UIImage image)
		{
			this.Film = film;

			this.Header.Text = film.TitleWithClassification;
			this.Description.Text = film.ShortDesc;
			this.Poster.Image = image;
			this.Poster.Layer.CornerRadius = 10f;
			this.Poster.Layer.MasksToBounds = true;
			this.Poster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Poster.Layer.Opaque = true;

			this.Duration.Text = String.Format ("Duration: {0} mins", film.Runtime);

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(0f, 0f, 50f, 30f), ratingConfig, Convert.ToDecimal(film.AverageRating));

			foreach(var sub in this.Rating.Subviews)
			{
				sub.RemoveFromSuperview();
			}

			// [Required] Add the view to the 
			this.Rating.AddSubview(ratingView);

			var reviewCount = new UILabel (new RectangleF (60f, 0f, 60f, 30f));
			reviewCount.Font = this.Duration.Font;
			reviewCount.Text = String.Format ("{0} ratings", film.Reviews.Count);
			this.Rating.AddSubview (reviewCount);
		}

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = image;
		}
	}
}
