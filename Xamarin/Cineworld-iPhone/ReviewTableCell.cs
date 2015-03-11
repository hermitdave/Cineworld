using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Cineworld;
using PDRatingSample;
using System.Drawing;

namespace CineworldiPhone
{
	partial class ReviewTableCell : UITableViewCell
	{
		public ReviewTableCell (IntPtr handle) : base (handle)
		{

		}

		public void UpdateCell(ReviewBase review)
		{
			this.TextLabel.Text = review.Reviewer;
			this.DetailTextLabel.Lines = 0;
			this.DetailTextLabel.Text = review.Review;
			this.DetailTextLabel.SizeToFit ();

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			var bounds = this.ContentView.Bounds;
			// Create the view.
			var ratingView = new PDRatingView(new RectangleF((float)bounds.Width-70, 4f, 50f, 30f), ratingConfig, Convert.ToDecimal(review.Rating));

			foreach(var sub in this.Subviews)
			{
				var rating = sub as PDRatingView;
				if(rating != null)
					sub.RemoveFromSuperview();
			}

			// [Required] Add the view to the 
			this.AddSubview(ratingView);
		}
	}
}
