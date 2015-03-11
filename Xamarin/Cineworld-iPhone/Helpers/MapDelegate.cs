using System;
using MapKit;
using UIKit;
using Foundation;

namespace CineworldiPhone
{
	public class MapDelegate : MKMapViewDelegate {
		protected string annotationIdentifier = "BasicAnnotation";
		UIButton detailButton; // avoid GC

		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, IMKAnnotation annotation)
		{
			// try and dequeue the annotation view
			MKAnnotationView annotationView = mapView.DequeueReusableAnnotation(annotationIdentifier);   
			// if we couldn't dequeue one, create a new one
			if (annotationView == null)
				annotationView = new MKPinAnnotationView(annotation, annotationIdentifier);
			else // if we did dequeue one for reuse, assign the annotation to it
				annotationView.Annotation = annotation;

			// configure our annotation view properties
			annotationView.CanShowCallout = true;
			(annotationView as MKPinAnnotationView).AnimatesDrop = true;
			(annotationView as MKPinAnnotationView).PinColor = MKPinAnnotationColor.Red;
			annotationView.Selected = true;

			// you can add an accessory view; in this case, a button on the right and an image on the left
			detailButton = UIButton.FromType(UIButtonType.DetailDisclosure);
			detailButton.TouchUpInside += (s, e) => {
				var cinemaAnnotation = (annotation as CinemaAnnotation);
				if(cinemaAnnotation == null)
					return;

				CinemaDetailsController cinemaDetails = cinemaAnnotation.Storyboard.InstantiateViewController ("CinemaDetailsController") as CinemaDetailsController;
				if (cinemaDetails != null) 
				{
					cinemaDetails.Cinema = cinemaAnnotation.Cinema;
					cinemaAnnotation.NavigationController.PushViewController (cinemaDetails, true);
				}
			};

			annotationView.RightCalloutAccessoryView = detailButton;

			annotationView.LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("Images/SmallLogo.png"));
			return annotationView;
		}
	}
}

