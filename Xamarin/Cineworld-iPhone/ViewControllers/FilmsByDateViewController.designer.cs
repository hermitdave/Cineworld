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
	[Register ("FilmsByDateViewController")]
	partial class FilmsByDateViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnSelectDate { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView DateView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView FilmsByDateTable { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblSelectedDate { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnSelectDate != null) {
				btnSelectDate.Dispose ();
				btnSelectDate = null;
			}
			if (DateView != null) {
				DateView.Dispose ();
				DateView = null;
			}
			if (FilmsByDateTable != null) {
				FilmsByDateTable.Dispose ();
				FilmsByDateTable = null;
			}
			if (lblSelectedDate != null) {
				lblSelectedDate.Dispose ();
				lblSelectedDate = null;
			}
		}
	}
}
