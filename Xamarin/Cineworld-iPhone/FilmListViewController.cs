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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.TableView.Source = FilmSource;
		}
	}
}
