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

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			(segue.DestinationViewController as CinemaDetailsController).Cinema = (sender as CinemaTableCell).Cinema;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var bounds = this.View.Bounds;

//			UITableView table = new UITableView(new RectangleF(10, 123, (float)bounds.Width-10, (float)bounds.Height-123));
//			table.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
//			table.Source = new CinemasTableSource (Application.Cinemas.Values);
//			table.SeparatorStyle = UITableViewCellSeparatorStyle.None;
//			this.View.AddSubview (table);

			this.CinemaListView.Source = new CinemasTableSource (Application.Cinemas.Values);

			var mapView = new MKMapView (new RectangleF(0, 128, (float)bounds.Width, (float)bounds.Height-128));
			mapView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			mapView.ShowsUserLocation = true;
			mapView.Delegate = new MapDelegate();
			mapView.Hidden = true;
			View.AddSubview(mapView);

			foreach (var cinema in Application.Cinemas) 
			{
				var cinemaLoc = new CinemaAnnotation (new CLLocationCoordinate2D (cinema.Value.Latitude, cinema.Value.Longitute), cinema.Value, this.Storyboard, this.NavigationController);
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
					this.CinemaListView.Hidden = false;
					mapView.Hidden = true;
				}
				else
				{
					this.CinemaListView.Hidden = true;
					mapView.Hidden = false;
				}
			};
		}
	}
}
