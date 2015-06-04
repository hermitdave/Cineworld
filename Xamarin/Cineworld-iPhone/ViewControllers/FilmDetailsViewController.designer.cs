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
	[Register ("FilmDetailsViewController")]
	partial class FilmDetailsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView FilmDetailsContainer { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl FilmSegments { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (FilmDetailsContainer != null) {
				FilmDetailsContainer.Dispose ();
				FilmDetailsContainer = null;
			}
			if (FilmSegments != null) {
				FilmSegments.Dispose ();
				FilmSegments = null;
			}
		}
	}
}
