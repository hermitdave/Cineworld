using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class SearchTableCell : UITableViewCell
	{
		public SearchResult SearchResult { get; set; }

		public SearchTableCell (IntPtr handle) : base (handle)
		{
			
		}

		public void UpdateCell(SearchResult searchRes, UIImage image)
		{
			this.SearchResult = searchRes;

			this.Poster.Hidden = this.CinemaImage.Hidden = true;

			if (searchRes.SearchObject is FilmInfo) 
			{
				this.Poster.Image = image;
				this.Poster.Layer.CornerRadius = 10f;
				this.Poster.Layer.MasksToBounds = true;
				this.Poster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
				this.Poster.Layer.Opaque = true;
				this.Poster.Hidden = false;
			} else 
			{
				this.CinemaImage.Layer.CornerRadius = 10f;
				this.CinemaImage.Layer.MasksToBounds = true;
				this.CinemaImage.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
				this.CinemaImage.Layer.Opaque = true;
				this.CinemaImage.Hidden = false;
			}
			this.Title.Text = searchRes.Name;
			this.Subtitle.Text = searchRes.Subtitle;
		}

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = image;
		}
	}
}
