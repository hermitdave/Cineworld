using Foundation;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using UIKit;
using PDRatingSample;
using System.Drawing;
using Cineworld;
using System.Collections.Generic;
using Factorymind.Components;
using ObjCRuntime;
using MapKit;
using CoreLocation;

namespace CineworldiPhone
{
	partial class CinemaDetailsController : UIViewController
	{
		public CinemaDetailsController (IntPtr handle) : base (handle)
		{
		}

		public CinemaInfo Cinema { get; set; }
		private List<FilmInfo> Films;

		Dictionary<DateTime, Dictionary<int, FilmInfo>> dateFilms = new Dictionary<DateTime, Dictionary<int, FilmInfo>>();

		internal IEnumerable<FilmInfo> GetFilmsPerformingForDate(DateTime userSelected)
		{
			if (dateFilms.Count == 0)
			{
				foreach (var film in this.Films)
				{
					foreach (var pi in film.Performances)
					{
						Dictionary<int, FilmInfo> filmList = null;
						if (!dateFilms.TryGetValue(pi.PerformanceTS.Date, out filmList))
						{
							filmList = new Dictionary<int, FilmInfo>();
							dateFilms.Add(pi.PerformanceTS.Date, filmList);
						}

						if (!filmList.ContainsKey(film.EDI))
						{
							var f = film.Clone();
							f.Performances = new List<PerformanceInfo>(film.Performances.Where(p => p.PerformanceTS.Date == pi.PerformanceTS.Date));
							filmList.Add(film.EDI, f);
						}
					}
				}
			}

			if (!dateFilms.ContainsKey(userSelected))
				return new List<FilmInfo>();

			return dateFilms[userSelected].Values;
		}

		FilmPerformancesTableSource LoadFilmData (DateTime date)
		{
			var filmsToday = GetFilmsPerformingForDate (date);
			var ViewByDateSource = new FilmPerformancesTableSource (this.Cinema, filmsToday);

			this.SelectedDate.Text = date.ToString ("ddd, dd MMM yyyy");
			if (date == DateTime.Today) {
				this.SelectedDate.Text = String.Format ("{0} - Today", this.SelectedDate.Text);
			}
			else
				if (date == DateTime.Today.AddDays (1)) {
					this.SelectedDate.Text = String.Format ("{0} - Tomorrow", this.SelectedDate.Text);
				}
			return ViewByDateSource;
		}

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.CinemaTitle.Title = this.Cinema.Name;

			this.BusyIndicator.StartAnimating ();
			this.BusyIndicator.Hidden = false;

			await new LocalStorageHelper ().GetCinemaFilmListings (this.Cinema.ID, false);

			this.BusyIndicator.StopAnimating ();
			this.BusyIndicator.Hidden = true;

			// Gather up the images to be used.
			RatingConfig ratingConfig = new RatingConfig(UIImage.FromFile("Images/Stars/empty.png"), UIImage.FromFile("Images/Stars/filled.png"), UIImage.FromFile("Images/Stars/chosen.png"));

			// Create the view.
			var ratingView = new PDRatingView(new RectangleF(20f, 0f, 50f, 30f), ratingConfig, Convert.ToDecimal(this.Cinema.AverageRating));

			// [Required] Add the view to the 
			this.CinemaGist.AddSubview(ratingView);

			var reviewCount = new UILabel (new RectangleF (80f, 0f, 60f, 30f));
			reviewCount.Font = UIFont.FromName ("HelveticaNeue", 12f);
			reviewCount.Text = String.Format ("{0} ratings", this.Cinema.Reviews.Count);
			this.CinemaGist.AddSubview (reviewCount);

			this.Address.Text = this.Cinema.FullAddress;

			this.Telephone.Text = this.Cinema.Telephone;

			this.WalkingDirections.TouchUpInside += Directions_TouchUpInside;
			this.DrivingDirections.TouchUpInside += Directions_TouchUpInside;

			this.ReviewsTable.Source = new ReviewsTableSource (this.Cinema.Reviews);
			this.ReviewsTable.ReloadData ();

			this.Films = Application.CinemaFilms[this.Cinema.ID];

			var date = DateTime.Today;

			this.FilmsByDateTable.Source = LoadFilmData (date);
			this.FilmsByDateTable.ReloadData ();

			var fmCalendar = new FMCalendar (this.FilmsByDateView.Bounds);

			View.BackgroundColor = UIColor.White;

			// Specify selection color
			fmCalendar.SelectionColor = UIColor.Red;

			// Specify today circle Color
			fmCalendar.TodayCircleColor = UIColor.Blue;

			// Customizing appearance
			fmCalendar.LeftArrow = UIImage.FromFile ("leftArrow.png");
			fmCalendar.RightArrow = UIImage.FromFile ("rightArrow.png");

			fmCalendar.MonthFormatString = "MMMM yyyy";

			// Shows Sunday as last day of the week
			fmCalendar.SundayFirst = false;

			// Mark with a dot dates that fulfill the predicate
			fmCalendar.IsDayMarkedDelegate = (d) => 
			{
				return this.dateFilms.ContainsKey(d.Date);
			};

			// Turn gray dates that fulfill the predicate
			fmCalendar.IsDateAvailable = (d) =>
			{
				return (d >= DateTime.Today);
			};

			fmCalendar.MonthChanged = (d) => 
			{
				Console.WriteLine ("Month changed {0}", d.Date);
			};

			fmCalendar.DateSelected += (d) => 
			{
				Console.WriteLine ("Date selected: {0}", d);

				this.FilmsByDateTable.Source = LoadFilmData (d.Date);
				this.FilmsByDateTable.ReloadData ();

				this.FilmsByDateTable.Hidden = this.SelectedDate.Hidden = DateSelectionButton.Hidden = false;
				fmCalendar.Hidden = true;
			};

			// Add FMCalendar to SuperView
			fmCalendar.Center = this.View.Center;
			this.FilmsByDateView.AddSubview (fmCalendar);
			fmCalendar.Hidden = true;

			this.DateSelectionButton.TouchUpInside += (sender, e) => 
			{
				this.FilmsByDateTable.Hidden = this.SelectedDate.Hidden = DateSelectionButton.Hidden = true;
				fmCalendar.Hidden = false;
			};

			this.FilmsByDateView.Hidden = false;

			var currentFilms = new AllFilmsTableSource(AllFilmsTableSource.FilmListingType.Current, this.Films);
			var upcomingFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Upcoming, this.Films);

			this.CinemaSegments.ValueChanged += (sender, e) => 
			{
				this.CinemaGist.Hidden = true;
				this.AllFilmsTable.Hidden = true;
				this.FilmsByDateView.Hidden = true;

				switch(this.CinemaSegments.SelectedSegment)
				{
					case 0:
					this.FilmsByDateTable.Source = LoadFilmData(fmCalendar.CurrentSelectedDate.Date);
					this.FilmsByDateTable.ReloadData();
					this.FilmsByDateView.Hidden = false;
					break;

					case 1:
					this.AllFilmsTable.Source = currentFilms;
					this.AllFilmsTable.ReloadData();
					this.AllFilmsTable.Hidden = false;
					break;

					case 2:
					this.AllFilmsTable.Source = upcomingFilms;
					this.AllFilmsTable.ReloadData();
					this.AllFilmsTable.Hidden = false;
					break;

					case 3:
					this.CinemaGist.Hidden = false;
					break;
				}
			};
		}

		void Directions_TouchUpInside (object sender, EventArgs e)
		{
			var location = new CLLocationCoordinate2D(this.Cinema.Latitude, this.Cinema.Longitute);
			MKPlacemarkAddress address = null;
			var placemark = new MKPlacemark (location, address);

			var mapItem = new MKMapItem (placemark);
			mapItem.Name = String.Format ("Cineworld {0}", this.Cinema.Name);

			var launchOptions = new MKLaunchOptions ();
			launchOptions.DirectionsMode = (sender == this.WalkingDirections) ? MKDirectionsMode.Walking : MKDirectionsMode.Driving;
			launchOptions.ShowTraffic = (sender == this.DrivingDirections);
			launchOptions.MapType = MKMapType.Standard;

			mapItem.OpenInMaps (launchOptions);
		}

		public override bool ShouldPerformSegue (string segueIdentifier, NSObject sender)
		{
			if (segueIdentifier != null && segueIdentifier.Equals ("PerfSegue")) 
			{
				PerformanceInfo perf = (sender as PerformanceCollectionViewCell).Performance;
				return perf.AvailableFuture;
			}

			return base.ShouldPerformSegue (segueIdentifier, sender);
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			TicketPurchaseController ticketPurchaseController = (segue.DestinationViewController as TicketPurchaseController);

			if (ticketPurchaseController != null) 
			{
				PerformanceInfo perf = (sender as PerformanceCollectionViewCell).Performance;
				ticketPurchaseController.Performance = perf;
			} 
			else 
			{
				PerformancesController performancesController = (segue.DestinationViewController as PerformancesController);
				performancesController.Showing = PerformancesController.ViewType.FilmDetails;
				performancesController.Cinema = this.Cinema;
				var filmTableCell = (sender as FilmTableCell);
				if (filmTableCell != null) 
				{
					performancesController.Film = filmTableCell.Film;
				} 
				else 
				{
					performancesController.Film = (sender as FilmPerformanceTableCell).Film;
				}
			}
		}

//		public override bool CanBecomeFirstResponder
//		{
//			get
//			{
//				return true;
//			}
//		}
//
//		public override bool CanPerform(Selector action, NSObject withSender)
//		{
//			bool canPerform = false;
//
//			switch(action.Name)
//			{
//				case "BuyTickets":
//				case "SendMessage":
//				case "SendEmail":
//					canPerform = true;
//					break;
//			}
//
//			return canPerform;
//		}
	}
}
