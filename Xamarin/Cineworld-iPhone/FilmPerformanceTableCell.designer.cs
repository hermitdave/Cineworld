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
	[Register ("FilmPerformanceTableCell")]
	partial class FilmPerformanceTableCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Header { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UICollectionView Performances { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView Poster { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (Header != null) {
				Header.Dispose ();
				Header = null;
			}
			if (Performances != null) {
				Performances.Dispose ();
				Performances = null;
			}
			if (Poster != null) {
				Poster.Dispose ();
				Poster = null;
			}
		}
	}
}
