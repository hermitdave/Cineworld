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
	[Register ("ReviewController")]
	partial class ReviewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIActivityIndicatorView BusyIndicator { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextView Review { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton Submit { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField User { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (BusyIndicator != null) {
				BusyIndicator.Dispose ();
				BusyIndicator = null;
			}
			if (Review != null) {
				Review.Dispose ();
				Review = null;
			}
			if (Submit != null) {
				Submit.Dispose ();
				Submit = null;
			}
			if (User != null) {
				User.Dispose ();
				User = null;
			}
		}
	}
}
