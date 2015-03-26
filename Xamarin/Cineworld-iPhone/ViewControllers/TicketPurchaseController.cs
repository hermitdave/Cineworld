using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class TicketPurchaseController : UIViewController
	{
		public PerformanceInfo Performance { get; set; }

		public TicketPurchaseController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.WebBrowser.LoadRequest(new NSUrlRequest(new NSUrl(this.Performance.MobileBookingUrl.OriginalString)));

			//this.WebBrowser.ScrollView.ScrollRectToVisible (new CoreGraphics.CGRect(0, 0, 320, 64), true);
		}
	}
}
