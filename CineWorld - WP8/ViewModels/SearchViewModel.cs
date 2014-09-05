using CineWorld.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CineWorld.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        public string SearchString { get; set; }

        public System.Collections.ObjectModel.ObservableCollection<SearchResult> SearchResults { get; private set; }

        public void SearchAsync()
        {
            List<SearchResult> matches = new List<SearchResult>();

            foreach (var f in App.Films.Values)
            {
                IEnumerable<CastInfo> casts = from cast in f.FilmCast
                                              where cast.Name.IndexOf(this.SearchString, StringComparison.CurrentCultureIgnoreCase) >= 0
                                              select cast;

                foreach (var c in casts)
                {
                    matches.Add(new SearchResult() { Name = c.Name, Subtitle = String.Format("{0} in {1}", c.Character, f.Title), SearchObject = f, Image = c.ProfilePicture });
                }
            }

            IEnumerable<FilmInfo> films = from film in App.Films.Values
                                          where film.Title.IndexOf(this.SearchString, StringComparison.CurrentCultureIgnoreCase) >= 0
                                          select film;

            foreach (var f in films)
            {
                matches.Add(new SearchResult() { Name = f.Title, Image = f.PosterUrl, SearchObject = f });
            }

            IEnumerable<CinemaInfo> cinemas = from cinema in App.Cinemas.Values
                                              where cinema.Name.IndexOf(this.SearchString, StringComparison.CurrentCultureIgnoreCase) >= 0
                                              select cinema;

            foreach (var c in cinemas)
            {
                matches.Add(new SearchResult() { Name = c.Name, SearchObject = c, Image = new Uri("Images/Background.png", UriKind.Relative) });
            }

            this.SearchResults = new System.Collections.ObjectModel.ObservableCollection<SearchResult>(matches);

            this.RaisePropertyChanged("SearchResults");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HandleUserSelection(object selection, NavigationService navService)
        {
            if (selection == null)
                return;

            if (selection is FilmInfo)
            {
                FilmInfo fi = selection as FilmInfo;
                FilmDetails.SelectedFilm = fi;

                navService.Navigate(new Uri("/FilmDetails.xaml", UriKind.Relative));
            }
            else if (selection is CinemaInfo)
            {
                CinemaInfo ci = selection as CinemaInfo;
                CinemaDetails.SelectedCinema = ci;

                navService.Navigate(new Uri("/CinemaDetails.xaml", UriKind.Relative));
            }
        }
    }
}
