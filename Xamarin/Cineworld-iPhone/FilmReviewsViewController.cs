using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class FilmReviewsViewController : UIViewController
	{
		public FilmInfo Film { get; set; }

		public FilmReviewsViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			UITableViewCell cell = new UITableViewCell (this.btnRateReview.Frame);
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.UserInteractionEnabled = false;
			this.btnRateReview.AddSubview (cell);

			this.tblReviews.Source = new ReviewsTableSource (this.Film.Reviews);
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			var reviewController = (segue.DestinationViewController as ReviewController);

			reviewController.Film = this.Film;
			reviewController.FilmReviewsViewController = this;

			base.PrepareForSegue (segue, sender);
		}

		public void ReviewSubmitted()
		{
			this.NavigationController.PopViewController(true);
		}
	}
}
