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
		List<FilmInfo> films = null;

		Dictionary<string, object> cellDictionary = new Dictionary<string, object> ();

		public enum FilmListingType
		{
			Current,
			Upcoming,
		}

		public AllFilmsTableSource (FilmListingType type)
		{
			films = new List<FilmInfo> ();

			foreach (var film in Application.Films.Values)
			{
				if ((type == FilmListingType.Current && film.Release <= DateTime.UtcNow) ||
				    (type == FilmListingType.Upcoming && film.Release > DateTime.UtcNow)) 
				{
					films.Add (film);
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

				if (placeholder is List<FilmTableCell>) {
					this.InvokeOnMainThread (delegate {
						foreach (FilmTableCell c in (placeholder as List<FilmTableCell>))
							c.UpdateCell(image);
					});
				} else {
					this.InvokeOnMainThread (delegate {
						var c = placeholder as FilmTableCell;
						c.UpdateCell(image);
					});
				}
			}
		}

		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 175;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return films.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier);

			// if there are no cells to reuse, create a new one
			//if (cell == null)
			//	cell = new FilmTableCell (cellIdentifier);

			//cell.ContentView.Bounds = new RectangleF (0, 0, 300, 300);

			//cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

			string url = films [indexPath.Row].PosterUrl == null ? null : films [indexPath.Row].PosterUrl.OriginalString;
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

			//cell.ImageView.Image = image;

			//cell.TextLabel.Lines = 0;
			//cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			//cell.TextLabel.Text = films[indexPath.Row].TitleWithClassification;

			//cell.DetailTextLabel.Lines = 0;
			//cell.DetailTextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			//cell.DetailTextLabel.Text = films [indexPath.Row].ShortDesc;

			if (cell != null) 
			{
				(cell as FilmTableCell).UpdateCell (films [indexPath.Row], image);
			}
			return cell;
		}
	}
}

