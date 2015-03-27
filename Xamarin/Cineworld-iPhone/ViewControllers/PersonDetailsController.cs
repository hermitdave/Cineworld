using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Cineworld;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using CoreGraphics;

namespace CineworldiPhone
{
	partial class PersonDetailsController : UIViewController
	{
		public CastInfo Cast { get; set; }

		public PersonDetailsController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.PersonDetailsSegment.Enabled = false;

			this.BusyIndicator.StartAnimating ();

			this.NavigationItem.Title = this.Cast.Name;

			Person person = null;

			try
			{
				TMDBService tmdbService = new TMDBService ();

				if (Application.TMDBConfig == null) 
				{
					Application.TMDBConfig = await tmdbService.GetConfig ();
				}

				person = await tmdbService.GetPersonDetails (this.Cast.ID);
			}
			catch 
			{
				UIAlertView alert = new UIAlertView ("Cineworld", "Error downloading data. Please try again later", null, "OK", null);
				alert.Show();

				return;
			}
			finally 
			{
				this.BioView.Hidden = false;
				this.BusyIndicator.StopAnimating ();
			}

			var height = 0;

			string url = Cast.ProfilePicture == null ? null : Cast.ProfilePicture.OriginalString;
			var image = ImageManager.Instance.GetImage (url);
			if (image == null) 
			{
				image = UIImage.FromFile ("Images/PlaceHolder.png");
			} 

			this.Poster.Image = image;
			this.Poster.Image = image; 
			this.Poster.Layer.CornerRadius = 10f;
			this.Poster.Layer.MasksToBounds = true;
			this.Poster.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			this.Poster.Layer.Opaque = true;

			DateTime dtBirthDay;
			if (DateTime.TryParseExact(person.Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtBirthDay))
			{
				UILabel birthday = new UILabel(new CGRect(0, height, 166, 20));
				birthday.Font = UIFont.FromName ("HelveticaNeue", 12f);
				birthday.Text = String.Format("Born: {0}", dtBirthDay.ToString("dd MMM yyyy"));

				this.MiscView.AddSubview(birthday);
				height += 30;
			}

			if (!String.IsNullOrWhiteSpace(person.PlaceOfBirth))
			{
				UILabel birthplace = new UILabel(new CGRect(0, height, 166, 20));
				birthplace.Font = UIFont.FromName ("HelveticaNeue", 12f);
				birthplace.Text = person.PlaceOfBirth;

				this.MiscView.AddSubview(birthplace);
				height += 30;
			}

			DateTime dtDeathDay;
			if (DateTime.TryParseExact(person.Deathday, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtDeathDay))
			{
				UILabel deathDay = new UILabel(new CGRect(0, height, 166, 20));
				deathDay.Font = UIFont.FromName ("HelveticaNeue", 12f);
				deathDay.Text = String.Format("Died: {0}", dtBirthDay.ToString("dd MMM yyyy"));

				this.MiscView.AddSubview(deathDay);
				height += 30;
			}

			if (!String.IsNullOrWhiteSpace (person.Biography)) 
			{
				UILabel biography = new UILabel (new CGRect(15, 0, 290, 1));
				biography.Font = UIFont.FromName ("HelveticaNeue", 12f);
				biography.Lines = 0;
				biography.Text = person.Biography;
				biography.SizeToFit ();

				UIScrollView scrollviewer = new UIScrollView (new CGRect (0, 183, 320, 207));
				scrollviewer.ContentSize = biography.Bounds.Size;
				scrollviewer.AddSubview (biography);

				this.BioView.AddSubview (scrollviewer);
			}
				
			if (person.Credits != null && person.Credits.Cast != null && person.Credits.Cast.Length > 0)
			{
				List<MovieCastInfo> movieRoles = new List<MovieCastInfo>();

				foreach (var movieRole in person.Credits.Cast.OrderByDescending(p => p.ReleaseDate))
				{
					movieRoles.Add(new MovieCastInfo(Application.TMDBConfig.Images.BaseUrl, "w185", movieRole));
				}

				CastFilmTableSource castFilmSource = new CastFilmTableSource (movieRoles);
				this.FilmsView.Source = castFilmSource;
				this.FilmsView.ReloadData ();
			}

			this.PersonDetailsSegment.ValueChanged += (sender, e) => 
			{
				switch(this.PersonDetailsSegment.SelectedSegment)
				{
					case 0:
					this.BioView.Hidden = false;
					this.FilmsView.Hidden = true;
					break;

					case 1:
					this.FilmsView.Hidden = false;
					this.BioView.Hidden = true;
					break;
				}
			};

			this.PersonDetailsSegment.Enabled = true;
		}
	}
}
