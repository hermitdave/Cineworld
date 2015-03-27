using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Drawing;
using CoreGraphics;
using ObjCRuntime;
using MessageUI;
using System.Collections.Generic;
using System.Linq;
using Cineworld;

namespace CineworldiPhone
{
	partial class FilmPerformanceTableCell : UITableViewCell
	{
		UIView performanceView = null;

		public FilmPerformanceTableCell (IntPtr handle) : base (handle)
		{
		}

		public FilmInfo Film { get; private set; }
		public CinemaInfo Cinema { get; private set; }

		//PerformanceInfo SelectedPerformance;

		//int cellCount = 0;

		public void UpdateCell(CinemaInfo cinema, FilmInfo film, UIImage image)
		{
			this.Cinema = cinema;
			this.Film = film;

			this.Header.Text = film.TitleWithClassification;
			this.ShortDesc.Text = film.ShortDesc;
			this.Poster.Image = ImageHelper.ResizeImage(image, 66, 100);

			this.Poster.Layer.CornerRadius = 5f;
			this.Poster.Layer.MasksToBounds = true;
			this.Poster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Poster.Layer.Opaque = true;

			var rows = (film.Performances.Count / 4);

			if (film.Performances.Count % 4 > 0)
				rows++;

			float height = rows * 50;

			if (performanceView == null) 
			{
				performanceView = new UIView (new CGRect (15, 85, 270, height));
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

			for (int i = 0; i < film.Performances.Count; i++) 
			{
				var perf = film.Performances [i];

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

				if (perf.AvailableFuture) 
				{
					btn.TouchUpInside += Performance_TouchUpInside;
					//this.Performances.AddGestureRecognizer (new UITapGestureRecognizer (HandleTapGesture));
				}

				performanceView.AddSubview (btn);
			}

			this.ContentView.AddSubview (performanceView);
		}

		void Performance_TouchUpInside (object sender, EventArgs e)
		{
			PerformanceButton btn = (sender as PerformanceButton);

			PerformanceInfo perf = btn.Performance;

			var ticketPurchaseController = Application.Storyboard.InstantiateViewController ("TicketPurchaseController") as TicketPurchaseController;
			ticketPurchaseController.Performance = perf;

			Application.NavigationController.PushViewController (ticketPurchaseController, true);
		}

//		void HandleTapGesture (UITapGestureRecognizer sender)
//		{
//			if (sender.State != UIGestureRecognizerState.Ended)
//				return;
//
//			CGPoint initialPinchPoint = sender.LocationInView (this.Performances);
//			NSIndexPath tappedCellPath = this.Performances.IndexPathForItemAtPoint (initialPinchPoint);
//
//			this.SelectedPerformance = this.Film.Performances [tappedCellPath.Row];
//
//			var menuController = UIMenuController.SharedMenuController;
//			var ticketMenuItem = new UIMenuItem ("Buy Tickets", new Selector ("BuyTickets"));
//
//			List<UIMenuItem> menuItems = new List<UIMenuItem> ();
//			menuItems.Add (ticketMenuItem);
//
//			if (MFMessageComposeViewController.CanSendText) 
//			{
//				var smsMenuItem = new UIMenuItem ("Message", new Selector ("SendMessage"));
//				menuItems.Add (smsMenuItem);
//			}
//
//			if (MFMailComposeViewController.CanSendMail) 
//			{
//				var emailMenuItem = new UIMenuItem ("Email", new Selector ("SendEmail"));
//				menuItems.Add (emailMenuItem);
//			}
//
//			//var location = gestureRecognizer.LocationInView (gestureRecognizer.View);
//			BecomeFirstResponder ();
//			menuController.MenuItems = menuItems.ToArray ();
//			menuController.SetTargetRect (new RectangleF ((float)initialPinchPoint.X, (float)initialPinchPoint.Y, 0, 0), sender.View);
//			menuController.MenuVisible = true;
//		}
//
//		[Export("BuyTickets")]
//		void BuyTickets (UIMenuController controller)
//		{
//			// navigate to webview
//		}
//
//		[Export("SendMessage")]
//		void SendMessage ()
//		{
//			// send sms / imessage
//			if (MFMessageComposeViewController.CanSendText)
//			{
//				var body = String.Format("Shall we go and see \"{0}\" at Cineworld {1} on {2} at {3}? Book here {4}", (string)SelectedPerformance.FilmTitle, Cinema.Name, DateTimeToStringConverter.ConvertData(SelectedPerformance.PerformanceTS), SelectedPerformance.TimeString, (SelectedPerformance.BookUrl).ToString());
//
//				var messageController = new MFMessageComposeViewController();
//				messageController.Body = body;
//
//				messageController.Finished += async (sender, e) => 
//				{
//					await e.Controller.DismissViewControllerAsync(true);
//				};
//
//				messageController.ShowViewController(messageController, null);
//			}
//		}
//
//		[Export("SendEmail")]
//		void SendEmail (UIMenuController controller)
//		{
//			// send email
//			if (MFMailComposeViewController.CanSendMail)
//			{
//				var body = String.Format("Shall we go and see \"{0}\" at Cineworld {1} on {2} at {3}? Book here {4}", (string)SelectedPerformance.FilmTitle, Cinema.Name, DateTimeToStringConverter.ConvertData(SelectedPerformance.PerformanceTS), SelectedPerformance.TimeString, (SelectedPerformance.BookUrl).ToString());
//
//				var messageController = new MFMailComposeViewController();
//				messageController.SetMessageBody(body, false);
//				messageController.SetSubject ("Movie at Cineworld?");
//
//				messageController.Finished += async (sender, e) => 
//				{
//					await e.Controller.DismissViewControllerAsync(true);
//				};
//
//				messageController.ShowViewController(messageController, null);
//			}
//		}
//

		public void UpdateCell(UIImage image)
		{
			this.Poster.Image = ImageHelper.ResizeImage(image, 66, 100);
		}
	}
}
