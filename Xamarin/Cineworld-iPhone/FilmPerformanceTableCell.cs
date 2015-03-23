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
		public FilmPerformanceTableCell (IntPtr handle) : base (handle)
		{
		}

		public FilmInfo Film { get; private set; }
		public CinemaInfo Cinema { get; private set; }
		PerformanceInfo SelectedPerformance;

		int cellCount = 0;

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

			List<PerformanceInfo> selected4Perfs = new List<PerformanceInfo> ();
			var availablePerfs = film.Performances.FindAll (p => p.AvailableFuture).Take (4);
			if (availablePerfs.Any ()) 
			{
				selected4Perfs.AddRange (availablePerfs);
			} 
			else 
			{
				selected4Perfs.AddRange(film.Performances.OrderByDescending(p => p.PerformanceTS).Take(4).OrderBy(p => p.PerformanceTS));
			}

//			var rows = (film.Performances.Count / 4);
//
//			if (film.Performances.Count % 4 > 0)
//				rows++;
//
//			float height = rows * 50;
//
//			var bounds = this.Performances.Bounds;
//			this.Performances.Frame = new RectangleF (15f, 90f, 271f, height);

			PerformanceCollectionSource performanceSource = new PerformanceCollectionSource (selected4Perfs);
			this.Performances.Source = performanceSource;
			this.Performances.ReloadData ();
			//this.Performances.SizeToFit ();

			//this.SizeToFit ();
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
