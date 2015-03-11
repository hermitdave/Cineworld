using System;
using CoreLocation;
using MapKit;
using UIKit;

namespace CineworldiPhone
{
	public class CinemaAnnotation : MKAnnotation
	{
		CLLocationCoordinate2D coordinate;
		public override CLLocationCoordinate2D Coordinate { get { return coordinate; } }

		public CinemaInfo Cinema{ get; private set;}
		public UIStoryboard Storyboard { get; private set; }
		public  UINavigationController NavigationController { get; private set; }


		public override string Title { get{ return this.Cinema.Name; }}
		public CinemaAnnotation (CLLocationCoordinate2D coordinate, CinemaInfo cinema, UIStoryboard storyboard, UINavigationController navigationController) {
			this.coordinate = coordinate;
			this.Cinema = cinema;
			this.Storyboard = storyboard;
			this.NavigationController = navigationController;
		}
	}
}

