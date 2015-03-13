using System;
using UIKit;
using System.Collections.Generic;

namespace CineworldiPhone
{
	public class PerformanceCollectionSource : UICollectionViewSource
	{
		string cellIdentifier = "PerformanceCollectionViewCell";

		List<PerformanceInfo> Performances = new List<PerformanceInfo>();

		public PerformanceCollectionSource (List<PerformanceInfo> perf) 
		{
			this.Performances.Clear ();
			this.Performances.AddRange (perf);
		}

		#region implemented abstract members of UICollectionViewDataSource

		public override UICollectionViewCell GetCell (UICollectionView collectionView, Foundation.NSIndexPath indexPath)
		{
			var cell = collectionView.DequeueReusableCell (cellIdentifier, indexPath) as PerformanceCollectionViewCell;

			cell.UpdateCell (this.Performances [indexPath.Row]);

			return cell;
		}

		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			return Performances.Count;
		}

		#endregion
	}
}

