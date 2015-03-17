using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;
using Cineworld;
using System.Drawing;

namespace CineworldiPhone
{
	partial class PerformanceTableCell : UITableViewCell
	{
		public PerformanceTableCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(List<PerformanceInfo> perfGroup)
		{
			this.PerformanceDate.Text = perfGroup [0].PerformanceTS.Date.ToLongDateString();

			PerformanceCollectionSource performanceSource = new PerformanceCollectionSource (perfGroup);
			this.Performances.Source = performanceSource;
			this.Performances.ReloadData ();

			var cellCount = perfGroup.Count;

			var rows = (cellCount / 4);

			if (cellCount % 4 > 0)
				rows++;

			var height = rows * 50;

			var b = this.Performances.Bounds;
			this.Performances.Bounds = new RectangleF ((float)b.Left, (float)b.Top, (float)b.Width, height);
		}
	}
}
