using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;
using System.Linq;

namespace CineworldiPhone
{
	partial class SearchController : UIViewController
	{
		public SearchController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.SearchText.ShouldReturn += (textField) => { 
				this.SearchButton_TouchUpInside(this, new EventArgs());
				return true; 
			};

			this.SearchButton.TouchUpInside += SearchButton_TouchUpInside;
		}

		void SearchButton_TouchUpInside (object sender, EventArgs e)
		{
			this.SearchText.ResignFirstResponder();

			var searchResults = this.Search (this.SearchText.Text);

			SearchResultsTableSource searchSource = new SearchResultsTableSource (searchResults);
			this.SearchResults.Source = searchSource;
			this.SearchResults.ReloadData ();
		}

		public List<SearchResult> Search(string searchText)
		{
			List<SearchResult> matches = new List<SearchResult>();

			foreach (var f in Application.Films.Values)
			{
				IEnumerable<CastInfo> casts = from cast in f.FilmCast
						where cast.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0
					select cast;

				foreach (var c in casts)
				{
					matches.Add(new SearchResult() { Name = c.Name, Subtitle = String.Format("{0} in {1}", c.Character, f.Title), SearchObject = f, Image = c.ProfilePicture });
				}
			}

			IEnumerable<FilmInfo> films = from film in Application.Films.Values
					where film.Title.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0
				select film;

			foreach (var f in films)
			{
				matches.Add(new SearchResult() { Name = f.Title, Image = f.PosterUrl, SearchObject = f });
			}

			IEnumerable<CinemaInfo> cinemas = from cinema in Application.Cinemas.Values
					where cinema.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0
				select cinema;

			foreach (var c in cinemas)
			{
				matches.Add(new SearchResult() { Name = c.Name, SearchObject = c, Image = new Uri("Images/Background.png", UriKind.Relative) });
			}

			return matches;
		}
	}
}
