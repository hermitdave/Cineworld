using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Drawing;

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
			this.ShortDesc.Text = film.ShortDesc;
			this.Poster.Image = image;

			this.Poster.Layer.CornerRadius = 5f;
			this.Poster.Layer.MasksToBounds = true;
			this.Poster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Poster.Layer.Opaque = true;

			PerformanceCollectionSource performanceSource = new PerformanceCollectionSource (film.Performances);
			this.Performances.Source = performanceSource;
			this.Performances.ReloadData ();

			var rows = (film.Performances.Count / 4);

			if (film.Performances.Count % 4 > 0)
				rows++;

			var height = rows * 50;

			var b = this.Performances.Bounds;
			this.Performances.Bounds = new RectangleF ((float)b.Left, (float)b.Top, (float)b.Width, height);
		}

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = image;
		}
	}
}
