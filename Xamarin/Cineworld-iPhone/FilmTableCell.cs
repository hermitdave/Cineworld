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

		static RatingConfig ratingConfig;

		public FilmTableCell(IntPtr handle) : base(handle)
		{
		}

		static FilmTableCell()
		{
			ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));
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

			PDRatingView ratingView;
			UILabel reviewCount;

			foreach(var sub in this.Rating.Subviews)
			{
				sub.RemoveFromSuperview ();
			}

			//if(this.Rating.Subviews.Length == 0)
			//{
				// Create the view.
				ratingView = new PDRatingView(new RectangleF(0f, 0f, 50f, 30f), ratingConfig, Convert.ToDecimal(film.AverageRating));

				// [Required] Add the view to the 
				this.Rating.AddSubview(ratingView);

				reviewCount = new UILabel (new RectangleF (60f, 0f, 60f, 30f));
				reviewCount.Font = this.Duration.Font;
				reviewCount.Text = String.Format ("{0} ratings", film.Reviews.Count);
				this.Rating.AddSubview (reviewCount);
			//}
			//else
			//{
			//	foreach(var sub in this.Rating.Subviews)
			//	{
			//		ratingView = sub as PDRatingView;
			//		if(ratingView != null)
			//		{
			//			ratingView.AverageRating = Convert.ToDecimal (film.AverageRating);
			//			break;
			//		}
			//
			//		reviewCount = sub as UILabel;
			//		if (reviewCount != null) 
			//		{
			//			reviewCount.Text = String.Format ("{0} ratings", film.Reviews.Count);
			//		}
			//	}
			//}
		}

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = image; //ImageHelper.ResizeImage(image, 92, 139);
		}
	}
}
