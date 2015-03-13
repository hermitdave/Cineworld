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
	[Register ("CinemaDetailsController")]
	partial class CinemaDetailsController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Address { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView AllFilmsTable { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIActivityIndicatorView BusyIndicator { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView CinemaGist { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl CinemaSegments { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UINavigationItem CinemaTitle { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton RateReview { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView ReviewsTable { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Telephone { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView ViewByDate { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (Address != null) {
				Address.Dispose ();
				Address = null;
			}
			if (AllFilmsTable != null) {
				AllFilmsTable.Dispose ();
				AllFilmsTable = null;
			}
			if (BusyIndicator != null) {
				BusyIndicator.Dispose ();
				BusyIndicator = null;
			}
			if (CinemaGist != null) {
				CinemaGist.Dispose ();
				CinemaGist = null;
			}
			if (CinemaSegments != null) {
				CinemaSegments.Dispose ();
				CinemaSegments = null;
			}
			if (CinemaTitle != null) {
				CinemaTitle.Dispose ();
				CinemaTitle = null;
			}
			if (RateReview != null) {
				RateReview.Dispose ();
				RateReview = null;
			}
			if (ReviewsTable != null) {
				ReviewsTable.Dispose ();
				ReviewsTable = null;
			}
			if (Telephone != null) {
				Telephone.Dispose ();
				Telephone = null;
			}
			if (ViewByDate != null) {
				ViewByDate.Dispose ();
				ViewByDate = null;
			}
		}
	}
}
