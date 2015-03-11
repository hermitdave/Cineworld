using System;
using System.Collections.Generic;
using UIKit;
using Cineworld;
using System.Collections;

namespace CineworldiPhone
{
	public class ReviewsTableSource : UITableViewSource {
		string cellIdentifier = "ReviewTableCell";
		IList reviews = null;

		//Dictionary<int, nfloat> cellHeightDictionary = new Dictionary<int, nfloat> ();

		public ReviewsTableSource (IList reviews)
		{
			this.reviews = reviews;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return reviews.Count;
		}

		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var review = (reviews[indexPath.Row] as ReviewBase);
			int len = String.IsNullOrWhiteSpace (review.Review) ? 0 : review.Review.Length;
			if (len == 0) {
				return 30;
			} else if (len < 70) {
				return 50;
			}

			return 70;
			//return cellHeightDictionary.ContainsKey(indexPath.Row) ? cellHeightDictionary [indexPath.Row] : 75;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier) as ReviewTableCell;

			cell.UpdateCell (reviews [indexPath.Row] as ReviewBase);

			return cell;
		}
	}
}

