using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Drawing;
using MapKit;
using CoreLocation;

namespace CineworldiPhone
{
	partial class CinemasViewController : UIViewController
	{
		public CinemasViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var bounds = this.View.Bounds;

			UITableView table = new UITableView(new RectangleF(0, 95, (float)bounds.Width, (float)bounds.Height-95));
			table.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			table.Source = new CinemasTableSource ();
			table.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.View.AddSubview (table);

			var mapView = new MKMapView (new RectangleF(0, 95, (float)bounds.Width, (float)bounds.Height-95));
			mapView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			mapView.ShowsUserLocation = true;
			mapView.Hidden = true;
			View.AddSubview(mapView);

			foreach (var cinema in Application.Cinemas) 
			{
				var cinemaLoc = new CinemaAnnotation (new CLLocationCoordinate2D(cinema.Value.Latitude,cinema.Value.Longitute), cinema.Value.Name);
				mapView.AddAnnotation(cinemaLoc);

			}

			var coords = new CLLocationCoordinate2D(51.507222, -0.1275);

			mapView.CenterCoordinate = coords;

			var span = new MKCoordinateSpan(MapHelper.MilesToLatitudeDegrees(10), MapHelper.MilesToLongitudeDegrees(10, coords.Latitude));
			mapView.Region = new MKCoordinateRegion(coords, span);

			this.CinemasSegments.ValueChanged += (sender, e) => 
			{
				if(this.CinemasSegments.SelectedSegment == 0)
				{
					table.Hidden = false;
					mapView.Hidden = true;
				}
				else
				{
					table.Hidden = true;
					mapView.Hidden = false;
				}
			};
		}
	}
}
