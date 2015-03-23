using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class YouTubeController : UIViewController
	{
		public const string YouTubeEmbedUrl = "https://www.youtube.com/embed/{0}";
		public const string YouTubeEmbedString = "<html><body><iframe width=\"300\" height=\"169\" src=\"{0}\" frameborder=\"0\" allowfullscreen></iframe></body></html>";

		public string YouTubeId { get; set; }

		public YouTubeController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();


			string trailerurl = String.Format (YouTubeEmbedUrl, this.YouTubeId);
			this.YouTubeView.LoadHtmlString (String.Format (YouTubeEmbedString, trailerurl), new NSUrl (trailerurl));
		}
	}
}
