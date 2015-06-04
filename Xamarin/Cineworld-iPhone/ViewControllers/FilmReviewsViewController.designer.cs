// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	[Register ("FilmReviewsViewController")]
	partial class FilmReviewsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnRateReview { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tblReviews { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnRateReview != null) {
				btnRateReview.Dispose ();
				btnRateReview = null;
			}
			if (tblReviews != null) {
				tblReviews.Dispose ();
				tblReviews = null;
			}
		}
	}
}
