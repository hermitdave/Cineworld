using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class FilmCinemasViewController : UITableViewController
	{
		public FilmInfo Film { get; set; }

		public FilmCinemasViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.TableView.Source = new CinemasTableSource (Application.FilmCinemas[this.Film.EDI]);
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			var destVC = (segue.DestinationViewController as ShowPerformancesViewController);
			destVC.Film = this.Film;
			destVC.Cinema = (sender as CinemaTableCell).Cinema;
			destVC.Showing = ShowPerformancesViewController.ViewType.CinemaDetails;
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			this.TableView.DeselectRow (this.TableView.IndexPathForSelectedRow, false);
		}
	}
}
