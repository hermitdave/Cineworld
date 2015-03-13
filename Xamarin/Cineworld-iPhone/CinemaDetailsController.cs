using Foundation;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using UIKit;
using PDRatingSample;
using System.Drawing;
using Cineworld;
using System.Collections.Generic;

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

			this.ReviewsTable.Source = new ReviewsTableSource (this.Cinema.Reviews);
			this.ReviewsTable.ReloadData ();

			this.Films = Application.CinemaFilms[this.Cinema.ID];

			var filmsToday = GetFilmsPerformingForDate (DateTime.Today);

			var ViewByDateSource = new FilmPerformancesTableSource (filmsToday);
			this.ViewByDate.Source = ViewByDateSource;
			this.ViewByDate.ReloadData ();

			var currentFilms = new AllFilmsTableSource(AllFilmsTableSource.FilmListingType.Current, this.Films);
			var upcomingFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Upcoming, this.Films);

			this.CinemaSegments.ValueChanged += (sender, e) => 
			{
				this.CinemaGist.Hidden = true;
				this.AllFilmsTable.Hidden = true;
				this.ViewByDate.Hidden = true;

				switch(this.CinemaSegments.SelectedSegment)
				{
					case 0:
					this.ViewByDate.Source = ViewByDateSource;
					this.ViewByDate.ReloadData();
					this.ViewByDate.Hidden = false;
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
	}
}
