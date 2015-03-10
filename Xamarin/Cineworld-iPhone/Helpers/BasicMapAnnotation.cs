using System;
using CoreLocation;
using MapKit;

namespace CineworldiPhone
{
	public class CinemaAnnotation : MKAnnotation
	{
		CLLocationCoordinate2D coordinate;
		public override CLLocationCoordinate2D Coordinate { get { return coordinate; } }
		string title;
		public override string Title { get{ return title; }}
		public CinemaAnnotation (CLLocationCoordinate2D coordinate, string title) {
			this.coordinate = coordinate;
			this.title = title;
		}
	}
}

