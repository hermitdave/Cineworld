using Cineworld;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineWorld.ViewModels
{
    public class FilmPosterViewModel : INotifyPropertyChanged
    {
        ObservableCollection<FilmInfo> allowedFilms = new ObservableCollection<FilmInfo>();
        ObservableCollection<FilmInfo> ignoredFilms = new ObservableCollection<FilmInfo>();

        public ObservableCollection<FilmInfo> AllowedFilms { get { return this.allowedFilms; } }

        public ObservableCollection<FilmInfo> IgnoredFilms { get { return this.ignoredFilms; } }

        public async Task LoadPosters()
        {
            HashSet<int> ignored = new HashSet<int>(Config.FilmPostersToIgnore);

            HashSet<string> uniqueposters = new HashSet<string>();

            object o = await new BaseStorageHelper().ReadMoviePosters();

            if (o == null || !(o is JObject))
                return;

            JObject data = (JObject)o;

            foreach (var item in data)
            {
                if (item.Value == null)
                    continue;

                if (uniqueposters.Contains((string)item.Value))
                    continue;

                uniqueposters.Add((string)item.Value);

                int tmdb = Int32.Parse(item.Key);
                FilmInfo fi = new FilmInfo() { TmdbId = tmdb, MediumPosterUrl = new Uri((string)item.Value, UriKind.Absolute) };
                if (ignored.Contains(tmdb))
                    this.ignoredFilms.Add(fi);
                else
                    this.allowedFilms.Add(fi);
            }

            this.OnPropertyChanged("AllowedFilms");
            this.OnPropertyChanged("IgnoredFilms");
        }

        public void IgnoreFilm(FilmInfo fi)
        {
            List<FilmInfo> filmsToIgnore = this.allowedFilms.Where(f => f.TmdbId == fi.TmdbId).ToList();

            if (filmsToIgnore != null)
            {
                List<int> films = new List<int>(Config.FilmPostersToIgnore);
                foreach (var film in filmsToIgnore)
                {
                    if (!films.Contains(film.TmdbId))
                    {
                        films.Add(film.TmdbId);

                        this.ignoredFilms.Add(film);
                        this.allowedFilms.Remove(film);
                    }
                }

                Config.FilmPostersToIgnore = films;

                this.OnPropertyChanged("AllowedFilms");
                this.OnPropertyChanged("IgnoredFilms");
            }
        }

        public void AllowFilm(FilmInfo fi)
        {
            List<FilmInfo> filmsToIgnore = this.ignoredFilms.Where(f => f.TmdbId == fi.TmdbId).ToList();

            if (filmsToIgnore != null)
            {
                List<int> films = new List<int>(Config.FilmPostersToIgnore);
                foreach (var film in filmsToIgnore)
                {
                    if (films.Contains(film.TmdbId))
                    {
                        films.Remove(film.TmdbId);

                        this.ignoredFilms.Remove(film);
                        this.allowedFilms.Add(film);
                    }
                }

                Config.FilmPostersToIgnore = films;
                this.OnPropertyChanged("AllowedFilms");
                this.OnPropertyChanged("IgnoredFilms");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
