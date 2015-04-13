using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using PDRatingSample;
using System.Drawing;
using Cineworld;
using CoreLocation;
using MapKit;
using System.Collections.Generic;
using Factorymind.Components;
using System.Linq;

namespace CineworldiPhone
{
	partial class CinemaDetailsViewController : UIViewController
	{
		public CinemaDetailsViewController (IntPtr handle) : base (handle)
		{
		}

		FilmsByDateViewController filmsByDateVC;
		FilmListViewController currentFilmsVC;
		FilmListViewController upcomingFilmsVC;
		CinemaInfoViewController cinemaInfoVC;

		public CinemaInfo Cinema { get; set; }

		private List<FilmInfo> Films;

		Dictionary<DateTime, Dictionary<int, FilmInfo>> dateFilms = new Dictionary<DateTime, Dictionary<int, FilmInfo>>();

		private void PopulateFilmPerformaceDictionary()
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
		}
//
//		FilmPerformancesTableSource LoadFilmData (DateTime date)
//		{
//			var filmsToday = GetFilmsPerformingForDate (date);
//			var ViewByDateSource = new FilmPerformancesTableSource (this.Cinema, filmsToday);
//
//			this.SelectedDate.Text = date.ToString ("ddd, dd MMM yyyy");
//			if (date == DateTime.Today) {
//				this.SelectedDate.Text = String.Format ("{0} - Today", this.SelectedDate.Text);
//			}
//			else
//				if (date == DateTime.Today.AddDays (1)) {
//					this.SelectedDate.Text = String.Format ("{0} - Tomorrow", this.SelectedDate.Text);
//				}
//			return ViewByDateSource;
//		}
//
		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.CinemaSegments.Enabled = false;

			this.BusyIndicator.StartAnimating ();

			try
			{
				await new LocalStorageHelper ().GetCinemaFilmListings (this.Cinema.ID, false);

				this.Films = Application.CinemaFilms[this.Cinema.ID];

				this.PopulateFilmPerformaceDictionary ();
			}
			catch
			{
				UIAlertView alert = new UIAlertView ("Cineworld", "Error downloading data. Please try again later", null, "OK", null);
				alert.Show();

				return;
			}
			finally 
			{
				this.BusyIndicator.StopAnimating ();
			}

			var filmsDateVC = this.GetFilmsByDateViewController();

			this.CinemaSegments.ValueChanged += (sender, e) => 
			{
				var currentVC = this.GetCurrentFilmsViewController();
				var upcomingVC = this.GetUpcomingFilmsViewController();
				var cinemaVC = this.GetCinemaInfoViewController();

				this.HideContainerView(filmsDateVC);
				this.HideContainerView(currentVC);
				this.HideContainerView(upcomingVC);
				this.HideContainerView(cinemaVC);

				switch(this.CinemaSegments.SelectedSegment)
				{
				case 0:
					this.ShowContainerView(filmsDateVC);
					break;

				case 1:
					this.ShowContainerView(currentVC);
					break;

				case 2:
					this.ShowContainerView(upcomingVC);
					break;

				case 3:
					this.ShowContainerView(cinemaVC);
					break;
				}
			};

			this.CinemaSegments.Enabled = true;

			this.ShowContainerView (filmsDateVC);

		}

		FilmsByDateViewController GetFilmsByDateViewController()
		{
			if (this.filmsByDateVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmsByDateViewController") as FilmsByDateViewController;

				vc.FilmPerformaceDictionary = this.dateFilms;
				vc.Cinema = this.Cinema;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, CinemaDetailsContainer.Frame.Width, CinemaDetailsContainer.Frame.Height);

				this.filmsByDateVC = vc;
			}

			return this.filmsByDateVC;
		}

		FilmListViewController GetCurrentFilmsViewController ()
		{
			if (this.currentFilmsVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmListViewController") as FilmListViewController;
				var currentFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Current, this.Films);
				vc.FilmSource = currentFilms;
				vc.Cinema = this.Cinema;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, CinemaDetailsContainer.Frame.Width, CinemaDetailsContainer.Frame.Height);

				this.currentFilmsVC = vc;
			}

			return this.currentFilmsVC;
		}

		FilmListViewController GetUpcomingFilmsViewController ()
		{
			if (this.upcomingFilmsVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmListViewController") as FilmListViewController;
				var upcomingFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Upcoming, this.Films);
				vc.FilmSource = upcomingFilms;
				vc.Cinema = this.Cinema;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, CinemaDetailsContainer.Frame.Width, CinemaDetailsContainer.Frame.Height);

				this.upcomingFilmsVC = vc;
			}

			return this.upcomingFilmsVC;
		}

		CinemaInfoViewController GetCinemaInfoViewController()
		{
			if (this.cinemaInfoVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController("CinemaInfoViewController") as CinemaInfoViewController;
				vc.Cinema = this.Cinema;
				vc.View.Frame = new CoreGraphics.CGRect(0, 0, CinemaDetailsContainer.Frame.Width, CinemaDetailsContainer.Frame.Height);

				this.cinemaInfoVC = vc;
			}

			return this.cinemaInfoVC;
		}

		private void ShowContainerView(UIViewController vc)
		{
			this.AddChildViewController(vc);

			this.CinemaDetailsContainer.AddSubview(vc.View);

			vc.DidMoveToParentViewController(this);
		}

		private void HideContainerView(UIViewController vc)
		{
			vc.WillMoveToParentViewController (null);

			vc.View.RemoveFromSuperview ();

			vc.RemoveFromParentViewController ();
		}


	}
}
