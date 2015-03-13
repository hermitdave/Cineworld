using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class PerformanceCollectionViewCell : UICollectionViewCell
	{
		public PerformanceInfo Performance { get; private set; }
		public PerformanceCollectionViewCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(PerformanceInfo performance)
		{
			this.Performance = performance;

			this.Time.Text = performance.TimeString;
			this.Type.Text = performance.Type;
		} 
	}
}
