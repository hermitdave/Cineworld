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
	[Register ("CinemaDetailsViewController")]
	partial class CinemaDetailsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIActivityIndicatorView BusyIndicator { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView CinemaDetailsContainer { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl CinemaSegments { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (BusyIndicator != null) {
				BusyIndicator.Dispose ();
				BusyIndicator = null;
			}
			if (CinemaDetailsContainer != null) {
				CinemaDetailsContainer.Dispose ();
				CinemaDetailsContainer = null;
			}
			if (CinemaSegments != null) {
				CinemaSegments.Dispose ();
				CinemaSegments = null;
			}
		}
	}
}
