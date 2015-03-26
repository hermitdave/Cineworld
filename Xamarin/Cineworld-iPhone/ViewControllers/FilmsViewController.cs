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

			this.FilmSegments.Enabled = false;

			var bounds = this.View.Bounds;

			var currentFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Current, Application.Films.Values);
			var upcomingFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Upcoming, Application.Films.Values);

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
				this.AllFilmsTable.ScrollRectToVisible(new CoreGraphics.CGRect(0, 0, 1, 1), false);
			};

			this.FilmSegments.Enabled = false;
		}
	}
}
