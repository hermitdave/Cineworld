using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class FilmCastTableCell : UITableViewCell
	{
		public CastInfo Cast { get; set; }

		public FilmCastTableCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(CastInfo cast, UIImage image)
		{
			this.Cast = cast;

			this.CastPoster.Image = image;
			this.CastDetail.Text = cast.Title;
		}

		public void UpdateCell(UIImage image)
		{
			this.CastPoster.Image = image;
		}
	}
}
