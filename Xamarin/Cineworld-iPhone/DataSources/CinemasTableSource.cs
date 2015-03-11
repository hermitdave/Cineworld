using System;
using System.Collections.Generic;
using UIKit;

namespace CineworldiPhone
{
	public class CinemasTableSource : UITableViewSource {
		string cellIdentifier = "CinemaTableCell";
		List<CinemaInfo> Cinemas = new List<CinemaInfo>();

		public CinemasTableSource (ICollection<CinemaInfo> cinemas)
		{
			Cinemas.Clear ();
			Cinemas.AddRange(cinemas);
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return Cinemas.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier) as CinemaTableCell;

			cell.UpdateCell (this.Cinemas [indexPath.Row]);

			return cell;

//			UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
//			// if there are no cells to reuse, create a new one
//			if (cell == null)
//				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
//
//			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
//			cell.TextLabel.Text = Cinemas[indexPath.Row].Name;
//			return cell;
		}
	}
}

