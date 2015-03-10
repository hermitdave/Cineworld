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

		Dictionary<string, object> cellDictionary = new Dictionary<string, object> ();

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
				object placeholder = this.cellDictionary [id];
				if (placeholder == null)
					return;
				this.cellDictionary.Remove (id);

				ProcessCell (placeholder, image);
			}
		}

		

		private void ProcessCell(List<FilmTableCell> cells, UIImage image)
		{
			if (cells == null)
				return;

			foreach (var cell in cells) 
			{
				ProcessCell (cell, image);
			}
		}

		private void ProcessCell(FilmTableCell cell, UIImage image)
		{
			if (cell == null)
				return;

			this.InvokeOnMainThread (delegate {
				cell.UpdateCell (image);
			});
		}

		private void ProcessCell(object obj, UIImage image)
		{
			if (obj == null)
				return;

			var c = obj as FilmTableCell;

			if (c == null) 
			{
				ProcessCell (obj as List<FilmTableCell>, image);
			} 
			else
			{
				ProcessCell (c, image);
			}
		}

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
			var cell = tableView.DequeueReusableCell (cellIdentifier);

			string url = Films [indexPath.Row].PosterUrl == null ? null : Films [indexPath.Row].PosterUrl.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				if (url != null) 
				{
					if (this.cellDictionary.ContainsKey (url)) 
					{
						var list = new List<UITableViewCell> ();
						list.Add (this.cellDictionary [url] as UITableViewCell);
						list.Add (cell);
						this.cellDictionary [url] = list;
					} 
					else 
					{
						this.cellDictionary.Add (url, cell);
					}
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

