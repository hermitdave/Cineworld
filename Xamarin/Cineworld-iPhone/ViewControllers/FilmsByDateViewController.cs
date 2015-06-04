using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;
using Factorymind.Components;

namespace CineworldiPhone
{
	partial class FilmsByDateViewController : UIViewController
	{
		public Dictionary<DateTime, Dictionary<int, FilmInfo>> FilmPerformaceDictionary { get; set; }
		public CinemaInfo Cinema { get; set; }

		public FilmsByDateViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.FilmsByDateTable.Source = LoadFilmData (DateTime.Today);
			this.FilmsByDateTable.ReloadData ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			var fmCalendar = new FMCalendar (new CoreGraphics.CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Height));

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
				return this.FilmPerformaceDictionary.ContainsKey(d.Date);
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
				this.FilmsByDateTable.ScrollRectToVisible(new CoreGraphics.CGRect(0, 0, 1, 1), false);

				this.FilmsByDateTable.Hidden = this.lblSelectedDate.Hidden = btnSelectDate.Hidden = false;
				fmCalendar.Hidden = true;
			};

			// Add FMCalendar to SuperView
			//fmCalendar.Center = this.View.Center;
			this.View.AddSubview (fmCalendar);
			fmCalendar.Hidden = true;

			this.btnSelectDate.TouchUpInside += (sender, e) => 
			{
				this.FilmsByDateTable.Hidden = this.lblSelectedDate.Hidden = this.btnSelectDate.Hidden = true;
				fmCalendar.Hidden = false;
			};

			this.DateView.Hidden = false;
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			if (this.FilmsByDateTable.IndexPathForSelectedRow != null) 
			{
				this.FilmsByDateTable.DeselectRow (this.FilmsByDateTable.IndexPathForSelectedRow, false);
			}
		}

		internal IEnumerable<FilmInfo> GetFilmsPerformingForDate(DateTime userSelected)
		{
			if (!this.FilmPerformaceDictionary.ContainsKey(userSelected))
				return new List<FilmInfo>();

			return this.FilmPerformaceDictionary[userSelected].Values;
		}

		FilmPerformancesTableSource LoadFilmData (DateTime date)
		{
			var filmsToday = GetFilmsPerformingForDate (date);
			var ViewByDateSource = new FilmPerformancesTableSource (this.Cinema, filmsToday);

			this.lblSelectedDate.Text = date.ToString ("ddd, dd MMM yyyy");
			if (date == DateTime.Today) {
				this.lblSelectedDate.Text = String.Format ("{0} - Today", this.lblSelectedDate.Text);
			}
			else
				if (date == DateTime.Today.AddDays (1)) {
					this.lblSelectedDate.Text = String.Format ("{0} - Tomorrow", this.lblSelectedDate.Text);
				}
			return ViewByDateSource;
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			var vc = (segue.DestinationViewController as ShowPerformancesViewController);
			vc.Film = (sender as FilmPerformanceTableCell).Film;
			vc.Cinema = this.Cinema;
		}
	}
}
