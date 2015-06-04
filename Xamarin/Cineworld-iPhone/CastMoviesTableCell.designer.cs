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
	[Register ("CastMoviesTableCell")]
	partial class CastMoviesTableCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Character { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Movie { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView Poster { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel ReleaseDate { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (Character != null) {
				Character.Dispose ();
				Character = null;
			}
			if (Movie != null) {
				Movie.Dispose ();
				Movie = null;
			}
			if (Poster != null) {
				Poster.Dispose ();
				Poster = null;
			}
			if (ReleaseDate != null) {
				ReleaseDate.Dispose ();
				ReleaseDate = null;
			}
		}
	}
}
