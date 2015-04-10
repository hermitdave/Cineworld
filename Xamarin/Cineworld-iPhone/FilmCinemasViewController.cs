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
	}
}
