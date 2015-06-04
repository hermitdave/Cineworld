using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;

namespace CineworldiPhone
{
	partial class FilmListViewController : UITableViewController
	{
		public FilmListViewController (IntPtr handle) : base (handle)
		{
		}

		public AllFilmsTableSource FilmSource { get; set; }
		public CinemaInfo Cinema { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.TableView.Source = FilmSource;
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			this.TableView.DeselectRow (this.TableView.IndexPathForSelectedRow, false);
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			var vc = (segue.DestinationViewController as ShowPerformancesViewController);
			vc.Film = (sender as FilmTableCell).Film;
			vc.Cinema = this.Cinema;
			vc.Showing = ShowPerformancesViewController.ViewType.FilmDetails;
		}
	}
}
