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
	[Register ("PersonDetailsController")]
	partial class PersonDetailsController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView BioView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView FilmsView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView MiscView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl PersonDetailsSegment { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView Poster { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (BioView != null) {
				BioView.Dispose ();
				BioView = null;
			}
			if (FilmsView != null) {
				FilmsView.Dispose ();
				FilmsView = null;
			}
			if (MiscView != null) {
				MiscView.Dispose ();
				MiscView = null;
			}
			if (PersonDetailsSegment != null) {
				PersonDetailsSegment.Dispose ();
				PersonDetailsSegment = null;
			}
			if (Poster != null) {
				Poster.Dispose ();
				Poster = null;
			}
		}
	}
}
