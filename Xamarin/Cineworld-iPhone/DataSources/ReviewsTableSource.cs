using System;
using System.Collections.Generic;
using UIKit;
using Cineworld;
using System.Collections;

namespace CineworldiPhone
{
	public class ReviewsTableSource : UITableViewSource {
		string cellIdentifier = "TableCell";
		IList reviews = null;
		public ReviewsTableSource (IList reviews)
		{
			this.reviews = reviews;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return reviews.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
			// if there are no cells to reuse, create a new one
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, cellIdentifier);

			cell.Accessory = UITableViewCellAccessory.None;
			var review = (reviews[indexPath.Row] as ReviewBase);
			cell.TextLabel.Text = review.Reviewer;
			cell.DetailTextLabel.Lines = 0;
			cell.DetailTextLabel.LineBreakMode = UILineBreakMode.CharacterWrap;
			cell.DetailTextLabel.Text = review.Review;
			return cell;
		}
	}
}

