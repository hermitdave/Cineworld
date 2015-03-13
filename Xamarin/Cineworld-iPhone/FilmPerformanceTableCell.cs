using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class FilmPerformanceTableCell : UITableViewCell
	{
		public FilmPerformanceTableCell (IntPtr handle) : base (handle)
		{
		}

		public FilmInfo Film { get; private set; }

		public void UpdateCell(FilmInfo film, UIImage image)
		{
			this.Film = film;

			this.Header.Text = film.TitleWithClassification;
			this.Poster.Image = image;

			PerformanceCollectionSource performanceSource = new PerformanceCollectionSource (film.PerformancesPrimary);
			this.Performances.Source = performanceSource;
			this.Performances.ReloadData ();
		}

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = image;
		}
	}
}
