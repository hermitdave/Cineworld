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
	[Register ("CinemaCollectionViewCell")]
	partial class CinemaCollectionViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel CinemaTitle { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel Distance { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CinemaTitle != null) {
				CinemaTitle.Dispose ();
				CinemaTitle = null;
			}
			if (Distance != null) {
				Distance.Dispose ();
				Distance = null;
			}
		}
	}
}
