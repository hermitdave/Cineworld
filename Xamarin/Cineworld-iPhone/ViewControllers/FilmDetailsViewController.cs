using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class FilmDetailsViewController : UIViewController
	{
		public FilmInfo Film { get; set; }

		public FilmDetailsViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.NavigationItem.Title = this.Film.TitleWithClassification;

			var detailsVC = this.GetFilmInfoViewController();

			this.FilmSegments.ValueChanged += (sender, e) => 
			{
				var castVC = this.GetFilmCastViewController();
				var reviewsVC = this.GetFilmReviewsViewController();
				var cinemasVC = this.GetFilmCinemasViewController();

				this.HideContainerView(detailsVC);
				this.HideContainerView(castVC);
				this.HideContainerView(reviewsVC);
				this.HideContainerView(cinemasVC);

				switch(this.FilmSegments.SelectedSegment)
				{
				case 0:
					this.ShowContainerView(detailsVC);
					break;

				case 1:
					this.ShowContainerView(castVC);
					break;

				case 2:
					this.ShowContainerView(reviewsVC);
					break;

				case 3:
					this.ShowContainerView(cinemasVC);
					break;
				}
			};

			this.ShowContainerView (detailsVC);
		}

		FilmInfoViewController _filmInfoVC = null;
		FilmInfoViewController GetFilmInfoViewController ()
		{
			if(this._filmInfoVC == null)
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmInfoViewController") as FilmInfoViewController;
				vc.Film = this.Film;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.FilmDetailsContainer.Frame.Width, this.FilmDetailsContainer.Frame.Height);

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

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.FilmDetailsContainer.Frame.Width, this.FilmDetailsContainer.Frame.Height);

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

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.FilmDetailsContainer.Frame.Width, this.FilmDetailsContainer.Frame.Height);

				this._filmReviewsVC = vc;
			}

			return this._filmReviewsVC;
		}

		FilmCinemasViewController _filmCinemasVC = null;
		FilmCinemasViewController GetFilmCinemasViewController()
		{
			if (this._filmCinemasVC == null) 
			{
				var vc = this.Storyboard.InstantiateViewController ("FilmCinemasViewController") as FilmCinemasViewController;
				vc.Film = this.Film;

				vc.View.Frame = new CoreGraphics.CGRect(0, 0, this.FilmDetailsContainer.Frame.Width, this.FilmDetailsContainer.Frame.Height);

				this._filmCinemasVC = vc;
			}

			return this._filmCinemasVC;
		}

		private void ShowContainerView(UIViewController vc)
		{
			this.AddChildViewController(vc);

			this.FilmDetailsContainer.AddSubview(vc.View);

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
