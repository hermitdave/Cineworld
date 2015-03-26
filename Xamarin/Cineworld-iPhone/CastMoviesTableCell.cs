using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Cineworld;

namespace CineworldiPhone
{
	partial class CastMoviesTableCell : UITableViewCell
	{
		public MovieCastInfo MovieCast { get; set; }

		public CastMoviesTableCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(MovieCastInfo movie, UIImage image)
		{
			this.MovieCast = movie;
			this.Poster.Image = image;
			this.ReleaseDate.Text = movie.ReleaseDate;
			this.Movie.Text = movie.Title;
			this.Character.Text = movie.Character;
		}

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = image;
		}
	}
}
