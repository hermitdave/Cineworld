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
	[Register ("FilmDetailsController")]
	partial class FilmDetailsController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView CinemasView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView FilmCastTable { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl FilmDetailSegments { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UINavigationItem FilmTitle { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIScrollView GistScrollViewer { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView Misc { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel OverviewData { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel OverviewLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton PlayTrailer { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView Poster { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton RateReviewButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView ReviewsView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView ReviewTable { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CinemasView != null) {
				CinemasView.Dispose ();
				CinemasView = null;
			}
			if (FilmCastTable != null) {
				FilmCastTable.Dispose ();
				FilmCastTable = null;
			}
			if (FilmDetailSegments != null) {
				FilmDetailSegments.Dispose ();
				FilmDetailSegments = null;
			}
			if (FilmTitle != null) {
				FilmTitle.Dispose ();
				FilmTitle = null;
			}
			if (GistScrollViewer != null) {
				GistScrollViewer.Dispose ();
				GistScrollViewer = null;
			}
			if (Misc != null) {
				Misc.Dispose ();
				Misc = null;
			}
			if (OverviewData != null) {
				OverviewData.Dispose ();
				OverviewData = null;
			}
			if (OverviewLabel != null) {
				OverviewLabel.Dispose ();
				OverviewLabel = null;
			}
			if (PlayTrailer != null) {
				PlayTrailer.Dispose ();
				PlayTrailer = null;
			}
			if (Poster != null) {
				Poster.Dispose ();
				Poster = null;
			}
			if (RateReviewButton != null) {
				RateReviewButton.Dispose ();
				RateReviewButton = null;
			}
			if (ReviewsView != null) {
				ReviewsView.Dispose ();
				ReviewsView = null;
			}
			if (ReviewTable != null) {
				ReviewTable.Dispose ();
				ReviewTable = null;
			}
		}
	}
}
