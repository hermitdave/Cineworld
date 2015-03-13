using System;
using System.Collections.Generic;
using System.Drawing;

using Foundation;
using UIKit;
using Cineworld;
using System.Threading.Tasks;

namespace CineworldiPhone
{
	public partial class Cineworld_iPhoneViewController : UIViewController
	{
		List<UIImage> images = new List<UIImage> ();

		public Cineworld_iPhoneViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		private async Task InstantiateTiles(bool bForce = false)
		{
			//await this.LoadFilmData();

			//await this.LoadPinnedAndFavouriteCinemas(bForce);

			//await LoadCinemasByLocation();
		}

		private static async Task Initialise(bool bForce = false)
		{
			LocalStorageHelper lsh = new LocalStorageHelper();
			await lsh.DownloadFiles(bForce);

			await lsh.DeserialiseObjects();
		}

		static UIImage ImageFromUrl(string uri)
		{
			using(var url = new NSUrl(uri))
			{
				using(var data = NSData.FromUrl(url))
				{
					return UIImage.LoadFromData (data);
				}
			}
		}

		public static UIImage RounderCorners (UIImage image, float width, float radius)
		{
			UIGraphics.BeginImageContext (new SizeF (width, width));
			var c = UIGraphics.GetCurrentContext ();

			//Note: You need to write the Device.IsRetina code yourself 
			//radius = Device.IsRetina ? radius * 2 : radius;

			c.BeginPath ();
			c.MoveTo (width, width / 2);
			c.AddArcToPoint (width, width, width / 2, width, radius);
			c.AddArcToPoint (0, width, 0, width / 2, radius);
			c.AddArcToPoint (0, 0, width / 2, 0, radius);
			c.AddArcToPoint (width, 0, width, width / 2, radius);
			c.ClosePath ();
			c.Clip ();

			image.Draw (new PointF (0, 0));
			var converted = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return converted;
		}


		private async Task LoadFilmData2()
		{
			List<Uri> posterUrls = await new BaseStorageHelper().GetImageList();

			var imageview = this.AllFilmsButton.ImageView;
			imageview.ContentMode = UIViewContentMode.ScaleAspectFill;
			var bounds = this.AllFilmsButton.Bounds;

			imageview.Frame = new RectangleF((float)bounds.Left, (float)bounds.Top, (float)bounds.Width, (float)bounds.Height);
			imageview.AnimationRepeatCount = 0;
			imageview.AnimationDuration = 10f;


			//foreach (var poster in posterUrls) 
			for (int i = 0; i < 5; i++) 
			{
				var img = ImageFromUrl (posterUrls [i].OriginalString);
				images.Add (img);

				imageview.AnimationImages = images.ToArray ();
			}

			this.AllFilmsButton.SetImage(images[0], UIControlState.Normal);
			imageview.StartAnimating ();


			//this.AllFilmsButton.SendSubviewToBack (imageview);
		}

		private async Task LoadFilmData()
		{
			List<Uri> posterUrls = await new BaseStorageHelper().GetImageList();

			var imageview = this.AllFilmsButton.ImageView;
			imageview.ContentMode = UIViewContentMode.ScaleAspectFill;
			var bounds = this.AllFilmsButton.Bounds;

			imageview.Frame = new RectangleF((float)bounds.Left, (float)bounds.Top, (float)bounds.Width, (float)bounds.Height);
			imageview.AnimationRepeatCount = 0;

			images.Clear ();

			ImageManager.Instance.ImageLoaded += HandleImageLoaded;

			//foreach (var poster in posterUrls) 
			for(int i = 0; i < posterUrls.Count; i++)
			{
				UIImage img = ImageManager.Instance.GetImage (posterUrls [i].OriginalString);

				if(img != null)
					images.Add (img);
			}

			if (images.Count > 0) 
			{
				imageview.AnimationImages = images.ToArray ();
				this.AllFilmsButton.SetImage (images[0], UIControlState.Normal);
				imageview.AnimationDuration = images.Count*2;

				imageview.StartAnimating ();
			}

			//this.AllFilmsButton.SendSubviewToBack (imageview);
		}

		void HandleImageLoaded (string id, UIImage image)
		{
			if (image != null) 
			{
				images.Add (image);

				this.InvokeOnMainThread (delegate {

					var imageview = this.AllFilmsButton.ImageView;
					imageview.StopAnimating();

					if(this.AllFilmsButton.CurrentImage == null)
						this.AllFilmsButton.SetImage(image, UIControlState.Normal);

					imageview.AnimationImages = images.ToArray();
				
					imageview.AnimationDuration = images.Count;

					imageview.StartAnimating();
				});
			}
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			ImageManager.Instance.ImageLoaded -= HandleImageLoaded;

			// set the View Controller that’s powering the screen we’re
			// transitioning to

			//var callHistoryContoller = segue.DestinationViewController as CinemasViewController;

			//set the Table View Controller’s list of phone numbers to the
			// list of dialed phone numbers

			//if (callHistoryContoller != null) {
			//	callHistoryContoller.PhoneNumbers = PhoneNumbers;
			//}
		}

		#region View lifecycle

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.

			//nuint cacheSizeMemory = 4*1024*1024; // 4MB
			//nuint cacheSizeDisk = 32*1024*1024; // 32MB

			//NSUrlCache.SharedCache = new NSUrlCache (cacheSizeMemory, cacheSizeDisk, "nsurlcache");

			this.BusyIndicator.StartAnimating ();
			this.BusyIndicator.Hidden = false;

			this.AllFilmsButton.Layer.CornerRadius = 10f;
			this.AllFilmsButton.Layer.MasksToBounds = true;
			this.AllFilmsButton.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.AllFilmsButton.Layer.Opaque = true;

			this.AllCinemasButton.Layer.CornerRadius = 10f;
			this.AllCinemasButton.Layer.MasksToBounds = true;
			this.AllCinemasButton.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.AllCinemasButton.Layer.Opaque = true;

			await Initialise (false);
			await LoadFilmData ();

			this.BusyIndicator.StopAnimating ();
			this.BusyIndicator.Hidden = true;

			this.AllFilmsButton.Enabled = this.AllCinemasButton.Enabled = true;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			this.AllFilmsButton.ImageView.StartAnimating ();
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);

			this.AllFilmsButton.ImageView.StopAnimating ();
		}

		#endregion
	}
}

