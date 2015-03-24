using System;
using System.Collections.Generic;
using System.Linq;
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
			this.AllFilmsButton.Layer.CornerRadius = 10f;
			this.AllFilmsButton.Layer.MasksToBounds = true;
			this.AllFilmsButton.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.AllFilmsButton.Layer.Opaque = true;

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
			this.BusyIndicator.Hidden = false;

			LocalStorageHelper lsh = new LocalStorageHelper();

			Console.WriteLine ("Start data download " + DateTime.Now.ToLongTimeString ());
			await lsh.DownloadFiles(bForce);

			Console.WriteLine ("Start data deserialisation " + DateTime.Now.ToLongTimeString ());
			await lsh.DeserialiseObjects();

			Task tFilmData = LoadFilmData ();

			Console.WriteLine ("Initialisation complete " + DateTime.Now.ToLongTimeString ());

			Application.UserLocation = locationManager.Location;

			LoadNearestCinemas ();

			Console.WriteLine ("Nearest cinemas loaded " + DateTime.Now.ToLongTimeString ());

			this.BusyIndicator.StopAnimating ();
			this.BusyIndicator.Hidden = true;

			this.AllFilmsButton.Enabled = this.AllCinemasButton.Enabled = true;

			await tFilmData;
		}

		int totalImageCount = 0;

		private async Task LoadFilmData()
		{
			await Task.Run (() => 
				{
				totalImageCount = Application.Films.Count;

				//var imageview = this.AllFilmsButton.ImageView;
				//imageview.ContentMode = UIViewContentMode.ScaleAspectFill;
				//var bounds = this.AllFilmsButton.Bounds;

				//imageview.Frame = new RectangleF ((float)bounds.Left, (float)bounds.Top, (float)bounds.Width, (float)bounds.Height);
				//imageview.AnimationRepeatCount = 0;

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

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			ImageManager.Instance.ImageLoaded -= HandleImageLoaded;

			var cinemaDetailsController = segue.DestinationViewController as CinemaDetailsController;
			if (cinemaDetailsController != null) 
			{
				cinemaDetailsController.Cinema = (sender as CinemaCollectionViewCell).Cinema;
			} 
			else 
			{
				SettingsController settingsController = (segue.DestinationViewController as SettingsController);
				if (settingsController != null) 
				{
					settingsController.MainViewController = this;
				}
			}

			base.PrepareForSegue (segue, sender);
		}

		#region View lifecycle

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Application.Storyboard = this.Storyboard;
			Application.NavigationController = this.NavigationController;

			this.SetButtonStyles ();

			locationManager.RequestWhenInUseAuthorization ();

			await Initialise (false);
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

