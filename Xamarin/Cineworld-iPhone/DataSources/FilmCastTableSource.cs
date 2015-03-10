using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;

namespace CineworldiPhone
{
	public class FilmCastTableSource : UITableViewSource {
		string cellIdentifier = "TableCell";
		List<CastInfo> cast = null;

		Dictionary<string, UITableViewCell> cellDictionary = new Dictionary<string, UITableViewCell> ();

		public FilmCastTableSource (List<CastInfo> filmCast)
		{
			cast = filmCast;

			ImageManager.Instance.ImageLoaded += HandleImageLoaded;
		}

		void HandleImageLoaded (string id, UIImage image)
		{
			if (image != null && this.cellDictionary.ContainsKey(id)) 
			{
				var tablecell = this.cellDictionary [id];
				this.cellDictionary.Remove (id);

				this.InvokeOnMainThread (delegate {
					tablecell.ImageView.Image = image;
					});
			}
		}

		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 175;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return cast.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier);

			// if there are no cells to reuse, create a new one
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);

			//cell.ContentView.Bounds = new RectangleF (0, 0, 300, 300);

			//cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

			string url = cast [indexPath.Row].ProfilePicture == null ? null : cast [indexPath.Row].ProfilePicture.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				if (url != null) 
				{
					this.cellDictionary.Add (url, cell);
				}
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			cell.ImageView.Frame = new RectangleF (10, 10, 92, 139);
			cell.ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
			cell.ImageView.Image = image;

			cell.TextLabel.Lines = 0;
			cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			cell.TextLabel.Text = String.Format("{0} as {1}", cast[indexPath.Row].Name, cast[indexPath.Row].Character);

			//cell.DetailTextLabel.Lines = 0;
			//cell.DetailTextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			//cell.DetailTextLabel.Text = films [indexPath.Row].ShortDesc;

			return cell;
		}
	}
}

