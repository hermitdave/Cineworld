using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using PDRatingSample;
using System.Drawing;
using Cineworld;

namespace CineworldiPhone
{
	partial class CinemaDetailsController : UIViewController
	{
		public CinemaDetailsController (IntPtr handle) : base (handle)
		{
		}

		public CinemaInfo Cinema { get; set; }

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.CinemaTitle.Title = this.Cinema.Name;

			await new LocalStorageHelper ().GetCinemaFilmListings (this.Cinema.ID, false);

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

			var allFilms = Application.CinemaFilms[this.Cinema.ID];

			var currentFilms = new AllFilmsTableSource(AllFilmsTableSource.FilmListingType.Current, allFilms);
			var upcomingFilms = new AllFilmsTableSource (AllFilmsTableSource.FilmListingType.Upcoming, allFilms);

			this.CinemaSegments.ValueChanged += (sender, e) => 
			{
				this.CinemaGist.Hidden = true;
				this.AllFilmsTable.Hidden = true;

				switch(this.CinemaSegments.SelectedSegment)
				{
					case 0:
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
