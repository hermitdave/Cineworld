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

			var rows = (perfGroup.Count / 4);

			if (perfGroup.Count % 4 > 0)
				rows++;

			float height = rows * 50;

			if (performanceView == null) 
			{
				performanceView = new UIView (new CGRect (15, 25, 290, height));
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

			//nfloat current = 0;

			for (int i = 0; i < perfGroup.Count; i++) 
			{
				var perf = perfGroup [i];
				UIButton btn = new UIButton (UIButtonType.RoundedRect);
				btn.Frame = new CGRect (currentCell * 70, currentRow * 50, 60, 40);

				btn.Layer.CornerRadius = 5f;
				btn.Layer.MasksToBounds = true;
				btn.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
				btn.Layer.Opaque = true;

				btn.BackgroundColor = UIColor.White;
				btn.Layer.BorderWidth = 0.5f;
				btn.Layer.BorderColor = perf.AvailableFuture ? UIColor.DarkGray.CGColor : UIColor.LightGray.CGColor;

				UILabel time = new UILabel (new CGRect (5, 3, 50, 20));
				time.Font = UIFont.FromName ("HelveticaNeue-Bold", 12f);
				time.Text = perf.TimeString;
				time.TextAlignment = UITextAlignment.Center;
				time.TextColor = perf.AvailableFuture ? UIColor.DarkGray : UIColor.LightGray;
				btn.AddSubview (time);

				UILabel type = new UILabel (new CGRect (5, 18, 50, 20));
				type.Font = UIFont.FromName ("HelveticaNeue", 12f);
				type.Text = perf.Type;
				type.TextAlignment = UITextAlignment.Center;
				type.TextColor = perf.AvailableFuture ? UIColor.DarkGray : UIColor.LightGray;
				btn.AddSubview (type);

				currentRow = (i + 1) / 4;
				if ((i + 1) % 4 == 0) 
				{
					currentCell = 0;
				} 
				else 
				{
					currentCell++;
				}

				btn.Tag = i;

				btn.TouchUpInside += Performance_TouchUpInside;
				//this.Performances.AddGestureRecognizer (new UITapGestureRecognizer (HandleTapGesture));

				performanceView.AddSubview (btn);
			}
		}

		void Performance_TouchUpInside (object sender, EventArgs e)
		{
			UIButton btn = (sender as UIButton);
			int item = (int)btn.Tag;

			PerformanceInfo perf = this.Performances [(int)item];

			var ticketPurchaseController = Application.Storyboard.InstantiateViewController ("TicketPurchaseController") as TicketPurchaseController;
			ticketPurchaseController.Performance = perf;

			Application.NavigationController.PushViewController (ticketPurchaseController, true);
		}
	}
}
