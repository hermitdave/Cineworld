using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Drawing;

namespace CineworldiPhone
{
	partial class FilmsViewController : UIViewController
	{
		public FilmsViewController (IntPtr handle) : base (handle)
		{
		
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			(segue.DestinationViewController as FilmDetailsController).Film = (sender as FilmTableCell).Film;
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var bounds = this.View.Bounds;

			var currentFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Current);
			var upcomingFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Upcoming);

			this.AllFilmsTable.Source = currentFilms;

			this.FilmSegments.ValueChanged += (sender, e) => 
			{
				if(this.FilmSegments.SelectedSegment == 0)
				{
					this.AllFilmsTable.Source = currentFilms;
				}
				else
				{
					this.AllFilmsTable.Source = upcomingFilms;
				}
				this.AllFilmsTable.ReloadData();
			};
		}
	}
}