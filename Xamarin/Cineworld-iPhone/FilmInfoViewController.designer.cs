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
	[Register ("FilmInfoViewController")]
	partial class FilmInfoViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnTrailer { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView imgPoster { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblOverview { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblOverviewHeader { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewMisc { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnTrailer != null) {
				btnTrailer.Dispose ();
				btnTrailer = null;
			}
			if (imgPoster != null) {
				imgPoster.Dispose ();
				imgPoster = null;
			}
			if (lblOverview != null) {
				lblOverview.Dispose ();
				lblOverview = null;
			}
			if (lblOverviewHeader != null) {
				lblOverviewHeader.Dispose ();
				lblOverviewHeader = null;
			}
			if (viewMisc != null) {
				viewMisc.Dispose ();
				viewMisc = null;
			}
		}
	}
}
