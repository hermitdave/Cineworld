using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;
using CoreGraphics;

namespace CineworldiPhone
{
	public class FilmCastTableSource : UITableViewSource {
		string cellIdentifier = "CastTableCell";
		List<CastInfo> _cast = null;

		Dictionary<string, FilmCastTableCell> cellDictionary = new Dictionary<string, FilmCastTableCell> ();


		public FilmCastTableSource (List<CastInfo> filmCast)
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
					var castCell = tablecell as FilmCastTableCell;
					if(castCell == null)
						return;

					castCell.UpdateCell(image);
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
			var cell = tableView.DequeueReusableCell (cellIdentifier) as FilmCastTableCell;

			string url = _cast [indexPath.Row].ProfilePicture == null ? null : _cast [indexPath.Row].ProfilePicture.OriginalString;
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
				(cell as FilmCastTableCell).UpdateCell (_cast [indexPath.Row], image);
			}
			return cell;
		}
	}
}

