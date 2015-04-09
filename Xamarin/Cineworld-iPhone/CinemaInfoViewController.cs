using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using CoreLocation;
using MapKit;
using PDRatingSample;
using System.Drawing;

namespace CineworldiPhone
{
	partial class CinemaInfoViewController : UIViewController
	{
		public CinemaInfo Cinema { get; set; }

		public CinemaInfoViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(15f, 0f, 50f, 30f), ratingConfig, Convert.ToDecimal(this.Cinema.AverageRating));

			// [Required] Add the view to the 
			this.CinemaGist.AddSubview(ratingView);

			var reviewCount = new UILabel (new RectangleF (75f, 0f, 60f, 30f));
			reviewCount.Font = UIFont.FromName ("HelveticaNeue", 12f);
			reviewCount.Text = String.Format ("{0} ratings", this.Cinema.Reviews.Count);
			this.CinemaGist.AddSubview (reviewCount);

			this.lblAddress.Text = this.Cinema.FullAddress;

			this.lblTelephone.Text = this.Cinema.Telephone;

			this.btnWalkingDirections.TouchUpInside += Directions_TouchUpInside;
			this.btnDrivingDirections.TouchUpInside += Directions_TouchUpInside;

			UITableViewCell cell = new UITableViewCell (this.btnRateReview.Frame);
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.UserInteractionEnabled = false;
			this.btnRateReview.AddSubview (cell);

			this.CinemaReviewsTable.Source = new ReviewsTableSource (this.Cinema.Reviews);
			this.CinemaReviewsTable.ReloadData ();
		}

		void Directions_TouchUpInside (object sender, EventArgs e)
		{
			var location = new CLLocationCoordinate2D(this.Cinema.Latitude, this.Cinema.Longitute);
			MKPlacemarkAddress address = null;
			var placemark = new MKPlacemark (location, address);

			var mapItem = new MKMapItem (placemark);
			mapItem.Name = String.Format ("Cineworld {0}", this.Cinema.Name);

			var launchOptions = new MKLaunchOptions ();
			launchOptions.DirectionsMode = (sender == this.btnWalkingDirections) ? MKDirectionsMode.Walking : MKDirectionsMode.Driving;
			launchOptions.ShowTraffic = (sender == this.btnDrivingDirections);
			launchOptions.MapType = MKMapType.Standard;

			mapItem.OpenInMaps (launchOptions);
		}
	}
}
