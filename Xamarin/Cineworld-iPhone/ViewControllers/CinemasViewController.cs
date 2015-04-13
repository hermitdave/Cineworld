using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Drawing;
using MapKit;
using CoreLocation;
using Cineworld;

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

			if (segue.DestinationViewController is CinemaDetailsViewController) 
			{
				(segue.DestinationViewController as CinemaDetailsViewController).Cinema = (sender as CinemaTableCell).Cinema;
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.CinemasSegment.Enabled = false;

			var bounds = this.View.Bounds;

			this.List.Source = new CinemasTableSource (Application.Cinemas.Values);

//			var mapView = new MKMapView (new RectangleF(0, 118, (float)bounds.Width, (float)bounds.Height-118-50));
//			mapView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
//			mapView.ShowsUserLocation = true;
//			mapView.Delegate = new MapDelegate();
//			mapView.Hidden = true;
//			View.AddSubview(mapView);

			this.Map.ShowsUserLocation = true;
			this.Map.Delegate = new MapDelegate();

			foreach (var cinema in Application.Cinemas) 
			{
				var cinemaLoc = new CinemaAnnotation (new CLLocationCoordinate2D (cinema.Value.Latitude, cinema.Value.Longitute), cinema.Value, this.Storyboard, this.NavigationController);
				this.Map.AddAnnotation(cinemaLoc);
			}

			CLLocationCoordinate2D coords;
			MKCoordinateSpan span;

			if (Application.UserLocation == null) 
			{
				if (Config.Region == Config.RegionDef.UK) 
				{
					coords = new CLLocationCoordinate2D (51.507222, -0.1275);
				} 
				else 
				{
					coords = new CLLocationCoordinate2D (53.347778, -6.259722);
				}

				span = new MKCoordinateSpan(MapHelper.MilesToLatitudeDegrees(100), MapHelper.MilesToLongitudeDegrees(100, coords.Latitude));
			}
			else
			{
				coords = Application.UserLocation.Coordinate;

				span = new MKCoordinateSpan(MapHelper.MilesToLatitudeDegrees(10), MapHelper.MilesToLongitudeDegrees(10, coords.Latitude));
			}

			this.Map.CenterCoordinate = coords;
			this.Map.Region = new MKCoordinateRegion(coords, span);
			
			this.CinemasSegment.ValueChanged += (sender, e) => 
			{
				if(this.CinemasSegment.SelectedSegment == 0)
				{
					this.List.Hidden = false;
					this.Map.Hidden = true;
				}
				else
				{
					this.List.Hidden = true;
					this.Map.Hidden = false;
				}
			};

			this.CinemasSegment.Enabled = true;
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			if (this.List.IndexPathForSelectedRow != null) 
			{
				this.List.DeselectRow (this.List.IndexPathForSelectedRow, false);
			}
		}
	}
}
