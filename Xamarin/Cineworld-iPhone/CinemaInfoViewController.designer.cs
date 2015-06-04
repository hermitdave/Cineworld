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
	[Register ("CinemaInfoViewController")]
	partial class CinemaInfoViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnDrivingDirections { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnRateReview { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnWalkingDirections { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView CinemaGist { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView CinemaReviewsTable { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblAddress { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblTelephone { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnDrivingDirections != null) {
				btnDrivingDirections.Dispose ();
				btnDrivingDirections = null;
			}
			if (btnRateReview != null) {
				btnRateReview.Dispose ();
				btnRateReview = null;
			}
			if (btnWalkingDirections != null) {
				btnWalkingDirections.Dispose ();
				btnWalkingDirections = null;
			}
			if (CinemaGist != null) {
				CinemaGist.Dispose ();
				CinemaGist = null;
			}
			if (CinemaReviewsTable != null) {
				CinemaReviewsTable.Dispose ();
				CinemaReviewsTable = null;
			}
			if (lblAddress != null) {
				lblAddress.Dispose ();
				lblAddress = null;
			}
			if (lblTelephone != null) {
				lblTelephone.Dispose ();
				lblTelephone = null;
			}
		}
	}
}
