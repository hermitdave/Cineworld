using System;
using UIKit;
using System.Collections.Generic;

namespace CineworldiPhone
{
	public class CinemaCollectionSource : UICollectionViewSource
	{
		string cellIdentifier = "CinemaCollectionViewCell";

		List<CinemaInfo> Cinemas = new List<CinemaInfo>();

		public CinemaCollectionSource (ICollection<CinemaInfo> cinemas) 
		{
			Cinemas.AddRange (cinemas);
		}

		#region implemented abstract members of UICollectionViewDataSource

		public override UICollectionViewCell GetCell (UICollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			var cell = collectionView.DequeueReusableCell (cellIdentifier, indexPath) as CinemaCollectionViewCell;

			cell.UpdateCell (this.Cinemas [indexPath.Row]);

			return cell;
		}

		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			return this.Cinemas.Count;
		}

		#endregion
	}
}

