using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using Cineworld;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using Android.Locations;
using System.Linq;

namespace Cineworld_Android
{
	[Activity(Label = "Cineworld", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : BaseActivity, ILocationListener
	{

		protected override int LayoutResource
		{
			get { return Resource.Layout.main; }
		}

		protected async override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Config.Initialise (this.BaseContext);

			var allFilmsButton = FindViewById<Button> (Resource.Id.AllFilmsButton);
			var allCinemasButton = FindViewById<Button> (Resource.Id.AllCinemasButton);

			allCinemasButton.Click += (object sender, EventArgs e) => {
				var intent = new Intent(this, typeof(CinemasActivity));
				StartActivity(intent);
			};


			await Initialise ();

			SupportActionBar.SetDisplayHomeAsUpEnabled(false);
			SupportActionBar.SetHomeButtonEnabled(false);
		}

		protected override void OnPause ()
		{
			base.OnPause ();

			ImageManager.Instance.ImageLoaded -= ImageManager_Instance_ImageLoaded;
		}

		static bool initComplete = false;
		private async Task Initialise(bool bForce = false)
		{
			var allFilmsButton = FindViewById<Button> (Resource.Id.AllFilmsButton);
			var allCinemasButton = FindViewById<Button> (Resource.Id.AllCinemasButton);

			//this.SettingsButton.SetTitle(Config.Region == Config.RegionDef.UK ? "United Kingdom" : "Ireland", UIControlState.Normal);

			allFilmsButton.Enabled = allCinemasButton.Enabled = false;

			//this.BusyIndicator.StartAnimating ();

			if (!initComplete) 
			{
				LocalStorageHelper lsh = new LocalStorageHelper ();

				Console.WriteLine ("Start data download " + DateTime.Now.ToLongTimeString ());
				await lsh.DownloadFiles (bForce);

				Console.WriteLine ("Start data deserialisation " + DateTime.Now.ToLongTimeString ());
				await lsh.DeserialiseObjects ();

				initComplete = true;
			}

			//this.SearchButton.Enabled = true;

			InitializeLocationManager ();

			Task tFilmData = LoadFilmData ();

			Console.WriteLine ("Initialisation complete " + DateTime.Now.ToLongTimeString ());

			//this.BusyIndicator.StopAnimating ();

			allFilmsButton.Enabled = allCinemasButton.Enabled = true;

			await tFilmData;
		}

		int totalImageCount = 0;
		List<Drawable> images = new List<Drawable> ();

		private async Task LoadFilmData()
		{
			await Task.Run (() => 
				{
					totalImageCount = CineworldApplication.Films.Count;

					images.Clear ();

					ImageManager.Instance.ImageLoaded += ImageManager_Instance_ImageLoaded;

					foreach (var film in CineworldApplication.Films.Values) {
						var posterUri = film.PosterImage;
						if (posterUri == null) {
							totalImageCount--;
							continue;
						}
						Drawable img = ImageManager.Instance.GetImage (posterUri.OriginalString);

						if (img != null)
							images.Add (img);

					}

					if (images.Count > 0) 
						this.SetAnimation();
				});
		}

		void ImageManager_Instance_ImageLoaded (string id, Drawable image)
		{
			if (image != null) 
			{
				images.Add (image);

				if (images.Count != totalImageCount)
					return;

				this.SetAnimation ();
			}
		}

		void SetAnimation()
		{
			var frameAnimation = new AnimationDrawable();
			foreach (var img in this.images) 
			{
				frameAnimation.AddFrame (img, 1000);
			}


			this.RunOnUiThread (() => {
				var imageButton = FindViewById<Button> (Resource.Id.AllFilmsButton);

				imageButton.SetBackgroundDrawable (frameAnimation);

				frameAnimation.Start ();
			});
		}

		LocationManager _locationManager;
		String _locationProvider;

		void InitializeLocationManager()
		{

			if (CineworldApplication.UserLocation == null) 
			{
				_locationManager = (LocationManager)GetSystemService (LocationService);
				Criteria criteriaForLocationService = new Criteria {
					Accuracy = Accuracy.Coarse
				};
				IList<string> acceptableLocationProviders = _locationManager.GetProviders (criteriaForLocationService, true);

				if (acceptableLocationProviders.Any ()) {
					_locationProvider = acceptableLocationProviders.First ();
				} else {
					_locationProvider = String.Empty;
				}

				_locationManager.RequestLocationUpdates (_locationProvider, 0, 0, this);
			} 
			else 
			{
				LoadNearestCinemas ();
			}
		}

		public void OnLocationChanged (Location location)
		{
			CineworldApplication.UserLocation = location;

			_locationManager.RemoveUpdates (this);

			LoadNearestCinemas ();
		}

		public void OnProviderDisabled (string provider)
		{
		}

		public void OnProviderEnabled (string provider)
		{

		}

		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{

		}

		void LoadNearestCinemas ()
		{
			if (CineworldApplication.UserLocation == null)
				return;

			IEnumerable<CinemaInfo> filteredCinemas = CineworldApplication.Cinemas.Values.Where(c => c.Longitute != 0 && c.Longitute != 0); //&& !PinnedCinemas.Contains(c.ID));

			if (!filteredCinemas.Any())
				return;

			int MaxCinemaCount = 6;

			int CountRequest = MaxCinemaCount; // - PinnedCinemas.Count;

			this.RunOnUiThread (() => {
				IEnumerable<CinemaInfo> cinemasByLocation = filteredCinemas.OrderBy (c => GeoMath.Distance (CineworldApplication.UserLocation.Latitude, CineworldApplication.UserLocation.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take (CountRequest);

				NearestCinemaAdapter ncAdapter = new NearestCinemaAdapter (this, cinemasByLocation, Resources.GetDrawable (Resource.Drawable.Background));

				GridView gridview = (GridView)this.FindViewById (Resource.Id.NearestCinemasView);
				gridview.Adapter = ncAdapter;
			});
		}
	}
}

