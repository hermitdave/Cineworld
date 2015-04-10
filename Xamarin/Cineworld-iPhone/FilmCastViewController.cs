using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class FilmCastViewController : UITableViewController
	{
		public FilmCastViewController (IntPtr handle) : base (handle)
		{
		}

		public FilmInfo Film { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.TableView.Source = new FilmCastTableSource (this.Film.FilmCast);
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			(segue.DestinationViewController as PersonDetailsController).Cast = (sender as FilmCastTableCell).Cast;
			base.PrepareForSegue (segue, sender);
		}
	}
}
