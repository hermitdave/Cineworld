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
				this.Poster.Hidden = false;
			} else 
			{
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
