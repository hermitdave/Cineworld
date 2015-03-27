using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Foundation;
using UIKit;
using Cineworld;
using System.Threading.Tasks;
using iAd;
using CoreGraphics;

namespace CineworldiPhone
{
	public partial class Cineworld_iPhoneViewController : UIViewController
	{
		List<UIImage> images = new List<UIImage> ();
		CoreLocation.CLLocationManager locationManager = new CoreLocation.CLLocationManager ();

		public Cineworld_iPhoneViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		void SetButtonStyles ()
		{
			UILabel filmsLabel = new UILabel (new CGRect (0, 0, 135, 42));
			filmsLabel.Font = UIFont.FromName ("HelveticaNeue", 15f);
			filmsLabel.Text = "All Films";
			filmsLabel.TextColor = UIColor.White;
			filmsLabel.TextAlignment = UITextAlignment.Center;
			filmsLabel.Alpha = 0.7f;
			filmsLabel.BackgroundColor = UIColor.DarkGray;
			this.AllFilmsButton.AddSubview (filmsLabel);

			this.AllFilmsButton.Layer.CornerRadius = 10f;
			this.AllFilmsButton.Layer.MasksToBounds = true;
			this.AllFilmsButton.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.AllFilmsButton.Layer.Opaque = true;

			UILabel cinemasLabel = new UILabel (new CGRect (0, 0, 135, 42));
			cinemasLabel.Font = UIFont.FromName ("HelveticaNeue", 15f);
			cinemasLabel.Text = "All Cinemas";
			cinemasLabel.TextColor = UIColor.White;
			cinemasLabel.TextAlignment = UITextAlignment.Center;
			cinemasLabel.Alpha = 0.7f;
			cinemasLabel.BackgroundColor = UIColor.DarkGray;
			this.AllCinemasButton.AddSubview (cinemasLabel);

			this.AllCinemasButton.Layer.CornerRadius = 10f;
			this.AllCinemasButton.Layer.MasksToBounds = true;
			this.AllCinemasButton.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.AllCinemasButton.Layer.Opaque = true;

			UITableViewCell cell = new UITableViewCell (this.SettingsButton.Frame);
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.UserInteractionEnabled = false;

			this.SettingsButton.AddSubview (cell);
		}

		private async Task Initialise(bool bForce = false)
		{
			this.SettingsButton.SetTitle(Config.Region == Config.RegionDef.UK ? "United Kingdom" : "Ireland", UIControlState.Normal);

			//this.AllFilmsButton.Enabled = this.AllCinemasButton.Enabled = false;

			this.BusyIndicator.StartAnimating ();

			LocalStorageHelper lsh = new LocalStorageHelper();

			Console.WriteLine ("Start data download " + DateTime.Now.ToLongTimeString ());
			await lsh.DownloadFiles(bForce);

			Console.WriteLine ("Start data deserialisation " + DateTime.Now.ToLongTimeString ());
			await lsh.DeserialiseObjects();

			this.SearchButton.Enabled = true;

			Task tFilmData = LoadFilmData ();

			Console.WriteLine ("Initialisation complete " + DateTime.Now.ToLongTimeString ());

			Application.UserLocation = locationManager.Location;

			LoadNearestCinemas ();

			Console.WriteLine ("Nearest cinemas loaded " + DateTime.Now.ToLongTimeString ());

			this.BusyIndicator.StopAnimating ();

			this.AllFilmsButton.Enabled = this.AllCinemasButton.Enabled = true;

			await tFilmData;
		}

		int totalImageCount = 0;

		private async Task LoadFilmData()
		{
			await Task.Run (() => 
				{
				totalImageCount = Application.Films.Count;

				images.Clear ();

				ImageManager.Instance.ImageLoaded += HandleImageLoaded;

				foreach (var film in Application.Films.Values) {
					var posterUri = film.PosterUrl;
					if (posterUri == null) {
						totalImageCount--;
						continue;
					}
					UIImage img = ImageManager.Instance.GetImage (posterUri.OriginalString);

					if (img != null)
						images.Add (img);
				}

				if (images.Count > 0) {
						InvokeOnMainThread ( () => {
							// manipulate UI controls
							this.AllFilmsButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
							this.AllFilmsButton.SetImage (images [0], UIControlState.Normal);
							this.AllFilmsButton.ImageView.AnimationImages = images.ToArray ();
							this.AllFilmsButton.ImageView.AnimationDuration = images.Count * 2;
							this.AllFilmsButton.ImageView.AnimationRepeatCount = 0;
							this.AllFilmsButton.ImageView.StartAnimating ();
						});
					
				}
			});
		}

		void HandleImageLoaded (string id, UIImage image)
		{
			if (image != null) 
			{
				images.Add (image);

				if (images.Count != totalImageCount)
					return;
				
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

		public async void RegionSelectionComplete(bool regionChanged)
		{
			this.NavigationController.PopViewController(true);

			if (regionChanged) 
			{
				this.NearestCinemas.Source = null;

				await Initialise ();
			}
		}

		void LoadNearestCinemas ()
		{
			if (Application.UserLocation == null)
				return;

			IEnumerable<CinemaInfo> filteredCinemas = Application.Cinemas.Values.Where(c => c.Longitute != 0 && c.Longitute != 0); //&& !PinnedCinemas.Contains(c.ID));

			if (!filteredCinemas.Any())
				return;

			int MaxCinemaCount = 6;

			int CountRequest = MaxCinemaCount; // - PinnedCinemas.Count;

			IEnumerable<CinemaInfo> cinemasByLocation = filteredCinemas.OrderBy(c => GeoMath.Distance(Application.UserLocation.Coordinate.Latitude, Application.UserLocation.Coordinate.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(CountRequest);

			CinemaCollectionSource cinemaSource = new CinemaCollectionSource (cinemasByLocation.ToList());

			this.NearestCinemas.Source = cinemaSource;
			this.NearestCinemas.ReloadData ();
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			ImageManager.Instance.ImageLoaded -= HandleImageLoaded;

			if(segue.DestinationViewController is CinemaDetailsController)
			{
				(segue.DestinationViewController as CinemaDetailsController).Cinema = (sender as CinemaCollectionViewCell).Cinema;
			} 
			else if(segue.DestinationViewController is SettingsController)
			{
				(segue.DestinationViewController as SettingsController).MainViewController = this;
			}

			base.PrepareForSegue (segue, sender);
		}

		#region View lifecycle

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var size = new CGSize (50, 36);

			Application.AvailableImageDefault = ImageHelper.ImageFromColor (UIColor.FromRGB(0, 122, 255), size);
			Application.AvailableImagePressed = ImageHelper.ImageFromColor (UIColor.FromRGB (0, 255, 255), size);
			Application.UnavailableImage = ImageHelper.ImageFromColor (UIColor.FromRGB (160, 160, 160), size);

			this.SearchButton.Enabled = false;

			AppDelegate appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
			appDelegate.Window.RootViewController.View.AddSubview (new ADBannerView (new CGRect (0, 518, 320, 50)) );

			Application.Storyboard = this.Storyboard;
			Application.NavigationController = this.NavigationController;

			this.SetButtonStyles ();

			locationManager.RequestWhenInUseAuthorization ();

			try
			{
				await Initialise (false);
			}
			catch
			{
				UIAlertView alert = new UIAlertView ("Cineworld", "Error downloading data. Please try again later", null, "OK", null);
				alert.Show();
			}
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

