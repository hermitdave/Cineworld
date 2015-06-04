using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;
using Cineworld;
using System.Drawing;
using CoreGraphics;

namespace CineworldiPhone
{
	partial class PerformanceTableCell : UITableViewCell
	{
		UIView performanceView = null;
		List<PerformanceInfo> Performances = null;

		public PerformanceTableCell (IntPtr handle) : base (handle)
		{
			
		}

		public void UpdateCell(List<PerformanceInfo> perfGroup)
		{
			this.Performances = perfGroup;
			this.PerformanceDate.Text = perfGroup [0].PerformanceTS.Date.ToLongDateString();

			var rows = (perfGroup.Count / 5);

			if (perfGroup.Count % 5 > 0)
				rows++;

			float height = rows * 46;

			if (performanceView == null) 
			{
				performanceView = new UIView (new CGRect (15, 30, 290, height));
				this.ContentView.AddSubview (performanceView);
			} 
			else 
			{
				foreach (var sub in performanceView.Subviews) 
				{
					sub.RemoveFromSuperview ();
				}
			}
			nfloat currentRow = 0;

			nfloat currentCell = 0;

			for (int i = 0; i < perfGroup.Count; i++) 
			{
				var perf = perfGroup [i];

				var btnRect = new CGRect (currentCell * 60, currentRow * 46, 50, 36);

				var btn = new PerformanceButton (btnRect, perf);
				currentRow = (i + 1) / 5;
				if ((i + 1) % 5 == 0) 
				{
					currentCell = 0;
				} 
				else 
				{
					currentCell++;
				}

				if(perf.AvailableFuture)
				{
					btn.TouchUpInside += Performance_TouchUpInside;
					//this.Performances.AddGestureRecognizer (new UITapGestureRecognizer (HandleTapGesture));
				}

				performanceView.AddSubview (btn);
			}
		}

		void Performance_TouchUpInside (object sender, EventArgs e)
		{
			PerformanceButton btn = (sender as PerformanceButton);

			PerformanceInfo perf = btn.Performance;

			var ticketPurchaseController = Application.Storyboard.InstantiateViewController ("TicketPurchaseController") as TicketPurchaseController;
			ticketPurchaseController.Performance = perf;

			Application.NavigationController.PushViewController (ticketPurchaseController, true);
		}
	}
}
