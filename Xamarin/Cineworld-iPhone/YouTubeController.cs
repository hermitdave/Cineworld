using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class YouTubeController : UIViewController
	{
		public string YouTubeId { get; set; }

		public YouTubeController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.YouTubePlayer.LoadRequest (new NSUrlRequest (new NSUrl (String.Format ("http://www.youtube.com/watch?v=", this.YouTubeId))));
		}
	}
}
