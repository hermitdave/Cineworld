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
		UISegmentedControl CinemasSegment { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView List { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		MapKit.MKMapView Map { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CinemasSegment != null) {
				CinemasSegment.Dispose ();
				CinemasSegment = null;
			}
			if (List != null) {
				List.Dispose ();
				List = null;
			}
			if (Map != null) {
				Map.Dispose ();
				Map = null;
			}
		}
	}
}
