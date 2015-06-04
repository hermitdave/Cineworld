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
	[Register ("PerformanceTableCell")]
	partial class PerformanceTableCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel PerformanceDate { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (PerformanceDate != null) {
				PerformanceDate.Dispose ();
				PerformanceDate = null;
			}
		}
	}
}
