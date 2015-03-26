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
			this.CastPoster.Layer.CornerRadius = 10f;
			this.CastPoster.Layer.MasksToBounds = true;
			this.CastPoster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.CastPoster.Layer.Opaque = true;

			this.CastDetail.Text = cast.Title;
		}

		public void UpdateCell(UIImage image)
		{
			this.CastPoster.Image = image;
		}
	}
}
