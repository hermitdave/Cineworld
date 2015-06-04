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
	[Register ("FilmTableCell")]
	partial class FilmTableCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Description { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Duration { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Header { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView Poster { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView Rating { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (Description != null) {
				Description.Dispose ();
				Description = null;
			}
			if (Duration != null) {
				Duration.Dispose ();
				Duration = null;
			}
			if (Header != null) {
				Header.Dispose ();
				Header = null;
			}
			if (Poster != null) {
				Poster.Dispose ();
				Poster = null;
			}
			if (Rating != null) {
				Rating.Dispose ();
				Rating = null;
			}
		}
	}
}
