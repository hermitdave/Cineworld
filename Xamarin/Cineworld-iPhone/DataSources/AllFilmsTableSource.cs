using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;

namespace CineworldiPhone
{
	public class AllFilmsTableSource : UITableViewSource {
		string cellIdentifier = "FilmTableCell";
		List<FilmInfo> Films = null;

		Dictionary<string, List<FilmTableCell>> cellDictionary = new Dictionary<string, List<FilmTableCell>> ();

		public enum FilmListingType
		{
			Current,
			Upcoming,
		}

		public AllFilmsTableSource (FilmListingType type, ICollection<FilmInfo> films)
		{
			Films = new List<FilmInfo> ();

			foreach (var film in films)
			{
				if ((type == FilmListingType.Current && film.Release <= DateTime.UtcNow) ||
				    (type == FilmListingType.Upcoming && film.Release > DateTime.UtcNow)) 
				{
					Films.Add (film);
				}
			}

			ImageManager.Instance.ImageLoaded += HandleImageLoaded;
		}

		void HandleImageLoaded (string id, UIImage image)
		{
			if (image != null && this.cellDictionary.ContainsKey(id)) 
			{
				var list = this.cellDictionary [id];
				if (list == null)
					return;
				this.cellDictionary.Remove (id);

				ProcessCell (id, list, image);
			}
		}

		

		private void ProcessCell(string id, List<FilmTableCell> cells, UIImage image)
		{
			if (cells == null)
				return;

			foreach (var cell in cells) 
			{
				ProcessCell (id, cell, image);
			}
		}

		private void ProcessCell(string id, FilmTableCell cell, UIImage image)
		{
			if (cell == null || !cell.Film.PosterUrl.OriginalString.Equals(id))
				return;

			this.InvokeOnMainThread (delegate {
				cell.UpdateCell (image);
			});
		}

//		private void ProcessCell(object obj, UIImage image)
//		{
//			if (obj == null)
//				return;
//
//			var c = obj as FilmTableCell;
//
//			if (c == null) 
//			{
//				ProcessCell (obj as List<FilmTableCell>, image);
//			} 
//			else
//			{
//				ProcessCell (c, image);
//			}
//		}

		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 175;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return Films.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier) as FilmTableCell;

			string url = Films [indexPath.Row].PosterUrl == null ? null : Films [indexPath.Row].PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				if (url != null) 
				{
					List<FilmTableCell> list = null;
					if (!this.cellDictionary.TryGetValue (url, out list)) 
					{
						list = new List<FilmTableCell> ();
						this.cellDictionary [url] = list;
					}

					list.Add (cell);
				}
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			if (cell != null) 
			{
				(cell as FilmTableCell).UpdateCell (Films [indexPath.Row], image);
			}
			return cell;
		}
	}
}

