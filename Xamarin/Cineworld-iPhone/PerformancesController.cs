using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Linq;
using System.Threading.Tasks;
using Cineworld;

namespace CineworldiPhone
{
	partial class PerformancesController : UIViewController
	{
		public enum ViewType
		{
			FilmDetails,
			CinemaDetails
		}

		public ViewType Showing { get; set; }

		public CinemaInfo Cinema { get; set; }

		public FilmInfo Film { get; set; }

		public PerformancesController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (this.Showing == ViewType.CinemaDetails) 
			{
				this.PerformancesSegment.RemoveSegmentAtIndex (2, false);
				this.PerformancesSegment.RemoveSegmentAtIndex (1, false);
				this.PerformancesSegment.RemoveSegmentAtIndex (0, false);

				//this.LoadCinemaDetails ();
			} 
			else 
			{
				this.PerformancesSegment.RemoveSegmentAtIndex (3, false);

				//this.LoadFilmDetails ();
			}

			await this.LoadPerformances();
		}

		async Task LoadPerformances ()
		{
			if (this.Film.Performances == null || this.Film.Performances.Count == 0) 
			{
				await new LocalStorageHelper ().GetCinemaFilmListings (this.Cinema.ID, false);

				this.Film = Application.CinemaFilms[this.Cinema.ID].First(f => f.EDI == this.Film.EDI);
			}
			this.PerformanceView.Source = new PerformancesTableSource (this.Film.Performances);
			this.PerformanceView.ReloadData ();
		}
	}
}
