using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class PerformancesViewController : UITableViewController
	{
		public FilmInfo Film { get; set; }
		public CinemaInfo Cinema { get; set; }

		public PerformancesViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.TableView.Source = new PerformancesTableSource (this.Film.Performances);
			this.TableView.ReloadData ();
		}
	}
}
