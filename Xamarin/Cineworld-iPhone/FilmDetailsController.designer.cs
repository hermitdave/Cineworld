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
		UISegmentedControl FilmDetailSegments { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UINavigationItem FilmTitle { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView GistView { get; set; }

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
		UIImageView Poster { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (FilmDetailSegments != null) {
				FilmDetailSegments.Dispose ();
				FilmDetailSegments = null;
			}
			if (FilmTitle != null) {
				FilmTitle.Dispose ();
				FilmTitle = null;
			}
			if (GistView != null) {
				GistView.Dispose ();
				GistView = null;
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
			if (Poster != null) {
				Poster.Dispose ();
				Poster = null;
			}
		}
	}
}
