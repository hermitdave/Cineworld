using System;
using System.Collections.Generic;
using UIKit;

namespace CineworldiPhone
{
	public class CinemasTableSource : UITableViewSource {
		string cellIdentifier = "TableCell";
		List<CinemaInfo> cinemas = null;

		public CinemasTableSource ()
		{
			cinemas = new List<CinemaInfo> (Application.Cinemas.Values);
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return cinemas.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
			// if there are no cells to reuse, create a new one
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);

			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.TextLabel.Text = cinemas[indexPath.Row].Name;
			return cell;
		}
	}
}

