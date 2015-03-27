using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;

namespace CineworldiPhone
{
	public class SearchResultsTableSource : UITableViewSource {
		string cellIdentifier = "SearchTableCell";
		List<SearchResult> SearchResults = null;

		Dictionary<string, List<SearchTableCell>> cellDictionary = new Dictionary<string, List<SearchTableCell>> ();

		public SearchResultsTableSource (List<SearchResult> searchResults)
		{
			this.SearchResults = searchResults;

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
				var list = this.cellDictionary [id];
				if (list == null)
					return;
				this.cellDictionary.Remove (id);

				ProcessCell (id, list, image);
			}
		}

		private void ProcessCell(string id, List<SearchTableCell> cells, UIImage image)
		{
			if (cells == null)
				return;

			foreach (var cell in cells) 
			{
				ProcessCell (id, cell, image);
			}
		}

		private void ProcessCell(string id, SearchTableCell cell, UIImage image)
		{
			if (cell == null || !cell.SearchResult.Image.OriginalString.Equals(id))
				return;

			this.InvokeOnMainThread (delegate {
				cell.UpdateCell (image);
			});
		}
			
		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			if (SearchResults [indexPath.Row].SearchObject is FilmInfo) 
			{
				return 149;
			}

			return 102;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return SearchResults.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier) as SearchTableCell;

			UIImage image = null;

			if (this.SearchResults [indexPath.Row].SearchObject is FilmInfo) 
			{
				string url = SearchResults [indexPath.Row].Image == null ? null : SearchResults [indexPath.Row].Image.OriginalString;
				image = ImageManager.Instance.GetImage (url);
				if (image == null) {
					if (url != null) {
						List<SearchTableCell> list = null;
						if (!this.cellDictionary.TryGetValue (url, out list)) {
							list = new List<SearchTableCell> ();
							this.cellDictionary [url] = list;
						}

						list.Add (cell);
					}
					image = UIImage.FromFile ("Images/PlaceHolder.png");
				} 
			} 
			else 
			{
				image = UIImage.FromFile (SearchResults [indexPath.Row].Image.OriginalString);
			}

			if (cell != null) 
			{
				cell.UpdateCell (SearchResults [indexPath.Row], image);
			}
			return cell;
		}
	}
}

