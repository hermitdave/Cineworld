using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using CoreLocation;
using Cineworld;

namespace CineworldiPhone
{
	partial class CinemaCollectionViewCell : UICollectionViewCell
	{
		public CinemaCollectionViewCell (IntPtr handle) : base (handle)
		{
		}

		public CinemaInfo Cinema { get; private set; }

		public void UpdateCell(CinemaInfo cinema)
		{
			this.Cinema = cinema;

			if (Application.UserLocation != null) 
			{
				var distance = GeoMath.Distance (Application.UserLocation.Coordinate.Latitude, Application.UserLocation.Coordinate.Longitude, cinema.Latitude, cinema.Longitute, GeoMath.MeasureUnits.Miles);

				this.Distance.Text = String.Format ("{0:N2} miles", distance);
			} 
			else 
			{
				
			}
			this.CinemaTitle.Text = cinema.Name;
			//this.CinemaTitle.SizeToFit ();

			ContentView.Layer.CornerRadius = 10f;
			ContentView.Layer.MasksToBounds = true;
			ContentView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			ContentView.Layer.Opaque = true;

		}
	}
}
