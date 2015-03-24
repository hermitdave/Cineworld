using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;

namespace CineworldiPhone
{
	public class FilmPerformancesTableSource : UITableViewSource {
		string cellIdentifier = "FilmPerformanceTableCell";
		List<FilmInfo> Films = null;
		CinemaInfo Cinema = null;

		Dictionary<string, object> cellDictionary = new Dictionary<string, object> ();

		public FilmPerformancesTableSource (CinemaInfo cinema, IEnumerable<FilmInfo> films)
		{
			this.Cinema = cinema;
			Films = new List<FilmInfo> (films);

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
			var film = this.Films [indexPath.Row];

			if (film == null || film.Performances.Count == 0)
				return 90;

			var rows = (film.Performances.Count / 4);

			if (film.Performances.Count % 4 > 0)
				rows++;

			var height = 135;

			if (rows > 1) 
			{
				rows--;

				height = 135 + (rows * 50);
			}

			return height;
//			
//			return 90 + (rows * 50);
//
//			//return 225;

//			return 135;
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
				(cell as FilmPerformanceTableCell).UpdateCell (Cinema, Films [indexPath.Row], image);
			}
			return cell;
		}
	}
}

