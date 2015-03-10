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
	[Register ("FilmCastTableCell")]
	partial class FilmCastTableCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel CastDetail { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView CastPoster { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CastDetail != null) {
				CastDetail.Dispose ();
				CastDetail = null;
			}
			if (CastPoster != null) {
				CastPoster.Dispose ();
				CastPoster = null;
			}
		}
	}
}
