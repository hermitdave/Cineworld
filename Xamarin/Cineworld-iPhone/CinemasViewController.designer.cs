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
	[Register ("CinemasViewController")]
	partial class CinemasViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView CinemaListView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl CinemasSegments { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CinemaListView != null) {
				CinemaListView.Dispose ();
				CinemaListView = null;
			}
			if (CinemasSegments != null) {
				CinemasSegments.Dispose ();
				CinemasSegments = null;
			}
		}
	}
}
