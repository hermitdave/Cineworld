using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Cineworld;
using System.Linq;

namespace CineworldiPhone
{
	partial class ShowPerformancesViewController : UIViewController
	{
		public CinemaInfo Cinema { get; set; }
		public FilmInfo Film { get; set; }

		public enum ViewType
		{
			FilmDetails,
			CinemaDetails
		}

		public ViewType Showing { get; set; }

		public ShowPerformancesViewController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.PerformancesSegment.Enabled = false;

			this.NavigationItem.Title = String.Format ("{0} at Cineworld {1}", this.Film.Title, this.Cinema.Name);

			this.BusyIndicator.StartAnimating ();

			try
			{
				await new LocalStorageHelper ().GetCinemaFilmListings (this.Cinema.ID, false);
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

			this.Film = Application.CinemaFilms[this.Cinema.ID].First(f => f.EDI == this.Film.EDI);

			var filmDetailsVC = this.GetFilmInfoViewController();
			var filmCastVC = this.GetFilmCastViewController();
			var filmReviewsVC = this.GetFilmReviewsViewController();
			var cinemaDetailsVC = this.GetCinemaInfoViewController();
			var performancesVC = this.GetPerformancesViewController ();

			if (this.Showing == ViewType.CinemaDetails) 
			{
				this.PerformancesSegment.RemoveSegmentAtIndex (2, false);
				this.PerformancesSegment.RemoveSegmentAtIndex (1, false);
				this.PerformancesSegment.RemoveSegmentAtIndex (0, false);

				this.ShowContainerView (cinemaDetailsVC);
			} 
			else 
			{
				this.PerformancesSegment.RemoveSegmentAtIndex (3, false);

				this.ShowContainerView (filmDetailsVC);
			}

			this.PerformancesSegment.SelectedSegment = 0;

			this.PerformancesSegment.ValueChanged += (sender, e) => 
			{
				this.HideContainerView(performancesVC);

				if(this.Showing == ViewType.CinemaDetails)
				{
					this.HideContainerView(cinemaDetailsVC);

					switch(this.PerformancesSegment.SelectedSegment)
					{
					case 0:
						this.ShowContainerView(cinemaDetailsVC);
						break;

					case 1:
						this.ShowContainerView(performancesVC);
						break;
					}
				}
				else
				{
					switch(this.PerformancesSegment.SelectedSegment)
					{
					case 0:
						this.ShowContainerView(filmDetailsVC);
						break;

					case 1:
						this.ShowContainerView(filmCastVC);
						break;

					case 2:
						this.ShowContainerView(filmReviewsVC);
						break;

					case 3:
						this.ShowContainerView(performancesVC);
						break;
					}
				}
			};

			this.PerformancesSegment.Enabled = true;
		}

		PerformancesViewController _performanecsVC = null;
		PerformancesViewController GetPerformancesViewController()
		{
			if (this._performanecsVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("PerformancesViewController") as PerformancesViewController;
				vc.Film = this.Film;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.CinemaDetailsContainer.Frame.Width, this.CinemaDetailsContainer.Frame.Height);

				this._performanecsVC = vc; 
			}

			return this._performanecsVC;
		}

		FilmInfoViewController _filmInfoVC = null;
		FilmInfoViewController GetFilmInfoViewController ()
		{
			if(this._filmInfoVC == null)
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmInfoViewController") as FilmInfoViewController;
				vc.Film = this.Film;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.CinemaDetailsContainer.Frame.Width, this.CinemaDetailsContainer.Frame.Height);

				this._filmInfoVC = vc;
			}

			return this._filmInfoVC;
		}

		FilmCastViewController _filmCastVC = null;
		FilmCastViewController GetFilmCastViewController()
		{
			if (this._filmCastVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmCastViewController") as FilmCastViewController;
				vc.Film = this.Film;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.CinemaDetailsContainer.Frame.Width, this.CinemaDetailsContainer.Frame.Height);

				this._filmCastVC = vc;
			}

			return this._filmCastVC;
		}

		FilmReviewsViewController _filmReviewsVC = null;
		FilmReviewsViewController GetFilmReviewsViewController()
		{
			if (this._filmReviewsVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmReviewsViewController") as FilmReviewsViewController;
				vc.Film = this.Film;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.CinemaDetailsContainer.Frame.Width, this.CinemaDetailsContainer.Frame.Height);

				this._filmReviewsVC = vc;
			}

			return this._filmReviewsVC;
		}

		CinemaInfoViewController _cinemaInfoVC = null;
		CinemaInfoViewController GetCinemaInfoViewController()
		{
			if (this._cinemaInfoVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController("CinemaInfoViewController") as CinemaInfoViewController;
				vc.Cinema = this.Cinema;
				vc.View.Frame = new CoreGraphics.CGRect(0, 0, CinemaDetailsContainer.Frame.Width, CinemaDetailsContainer.Frame.Height);

				this._cinemaInfoVC = vc;
			}

			return this._cinemaInfoVC;
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
