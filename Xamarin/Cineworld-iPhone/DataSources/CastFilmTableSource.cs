using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;
using CoreGraphics;
using Cineworld;

namespace CineworldiPhone
{
	public class CastFilmTableSource : UITableViewSource {
		string cellIdentifier = "CastMoviesTableCell";
		List<MovieCastInfo> _cast = null;

		Dictionary<string, CastMoviesTableCell> cellDictionary = new Dictionary<string, CastMoviesTableCell> ();


		public CastFilmTableSource (List<MovieCastInfo> filmCast)
		{
			_cast = filmCast;

			ImageManager.Instance.ImageLoaded += HandleImageLoaded;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			ImageManager.Instance.ImageLoaded -= HandleImageLoaded;
		}

		void HandleImageLoaded (string id, UIImage image)
		{
			if (image != null && this.cellDictionary.ContainsKey(id)) 
			{
				var tablecell = this.cellDictionary [id];
				this.cellDictionary.Remove (id);

				this.InvokeOnMainThread (delegate {
					if(tablecell == null || !tablecell.MovieCast.PosterUrl.OriginalString.Equals(id))
						return;

					tablecell.UpdateCell(image);
					});
			}
		}

		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 150;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return _cast.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier) as CastMoviesTableCell;

			string url = _cast [indexPath.Row].PosterUrl == null ? null : _cast [indexPath.Row].PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				if (url != null) 
				{
					this.cellDictionary[url] = cell;
				}
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			if (cell != null) 
			{
				cell.UpdateCell (_cast [indexPath.Row], image);
			}
			return cell;
		}
	}
}

