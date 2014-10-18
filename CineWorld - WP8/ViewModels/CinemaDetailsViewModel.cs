using Cineworld;
using CineWorld.Models;
using CineWorld.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineWorld.ViewModels
{
    public class CinemaDetailsViewModel : INotifyPropertyChanged
    {
        public CinemaDetailsViewModel()
        {
            this.GroupCurrent = new ObservableCollection<Group<FilmInfo>>();
            this.GroupUpcoming = new ObservableCollection<Group<FilmInfo>>();
            this.GroupFilmsForDate = new ObservableCollection<Group<FilmInfo>>();

            this.Current = new ObservableCollection<FilmInfo>();
            this.Upcoming = new ObservableCollection<FilmInfo>();
            this.FilmsForDate = new ObservableCollection<FilmInfo>();
            
        }

        public CinemaInfo CinemaDetails { get; set; }

        FilmData filmData = null;

        public bool Initialised { get; set; }

        public void Initialise(CinemaInfo selectedCinema, List<FilmInfo> films)
        {
            this.CinemaDetails = selectedCinema;
            this.filmData = new FilmData(films);

            List<FilmInfo> currentFilms = new List<FilmInfo>();
            List<FilmInfo> upcomingFilms = new List<FilmInfo>();

            foreach (var film in films)
            {
                if (film.Release <= DateTime.UtcNow)
                    currentFilms.Add(film);
                else
                    upcomingFilms.Add(film);  
            }

            foreach(var currentFilm in currentFilms)
            {
                this.Current.Add(currentFilm);
            }

            foreach(var upcomingFilm in upcomingFilms)
            {
                this.Upcoming.Add(upcomingFilm);
            }

            this.GroupCurrent = new ObservableCollection<Group<FilmInfo>>(new FilmData(currentFilms).GetGroupsByLetter());

            this.GroupUpcoming = new ObservableCollection<Group<FilmInfo>>(new FilmData(upcomingFilms).GetGroupsByLetter());

            this.SetFilmsForDate(DateTime.Today);

            this.RaisePropertyChanged("CinemaDetails");
            this.RaisePropertyChanged("Current");
            this.RaisePropertyChanged("Upcoming");
            this.RaisePropertyChanged("FilmsForDate");
            this.RaisePropertyChanged("GroupCurrent");
            this.RaisePropertyChanged("GroupUpcoming");
            this.RaisePropertyChanged("GroupFilmsForDate");
            this.RaisePropertyChanged("FilmAppointmentSource");
            this.RaisePropertyChanged("FirstCinemaDate");
            this.RaisePropertyChanged("LastCinemaDate");
            this.RaisePropertyChanged("UserSelectedDate");

            this.Initialised = true;
        }

        public ObservableCollection<FilmInfo> Current { get; private set; }
        public ObservableCollection<FilmInfo> Upcoming { get; private set; }
        public ObservableCollection<FilmInfo> FilmsForDate { get; private set; }

        public ObservableCollection<Group<FilmInfo>> GroupCurrent { get; private set; }
        public ObservableCollection<Group<FilmInfo>> GroupUpcoming { get; private set; }
        public ObservableCollection<Group<FilmInfo>> GroupFilmsForDate { get; private set; }

        public bool SetFilmsForDate(DateTime dtSelected)
        {
            this.FilmsForDate.Clear();
            foreach(var filmForDate in this.filmData.GetForDate(dtSelected))
            {
                this.FilmsForDate.Add(filmForDate);
            }

            var selectedDayFilms = this.filmData.GetGroupForDate(dtSelected).ToList();

            if(selectedDayFilms.Count > 0)
            {
                this.GroupFilmsForDate.Clear();

                foreach (var entry in selectedDayFilms)
                {
                    this.GroupFilmsForDate.Add(entry);
                }

                this.UserSelectedDate = dtSelected;
                this.RaisePropertyChanged("FilmsForDate");
            }
            return selectedDayFilms.Count > 0;
        }

        private FilmAppointmentSource _filmAppointmentSource;
        public FilmAppointmentSource FilmAppointmentSource
        {
            get
            {
                if (_filmAppointmentSource == null && this.filmData != null)
                {
                    var _filmAppointments = new List<FilmAppointment>();
                    foreach (var entry in this.filmData.GetGroupsByDate())
                    {
                        _filmAppointments.Add(new FilmAppointment()
                        {
                            StartDate = ((DateTime)entry.GroupTitle).AddHours(1),
                            EndDate = ((DateTime)entry.GroupTitle).AddHours(2),
                            Subject = "a"
                        });
                    }

                    _filmAppointmentSource = new FilmAppointmentSource(_filmAppointments);
                }

                return _filmAppointmentSource;
            }
        }

        public DateTime FirstCinemaDate
        {
            get
            {
                var entry = this.filmData == null ? null : this.filmData.GetGroupsByDate().FirstOrDefault();

                if (entry == null)
                    return DateTime.Today;

                return (DateTime)entry.GroupTitle;
            }
        }

        public DateTime LastCinemaDate
        {
            get
            {
                var entry = this.filmData == null ? null : this.filmData.GetGroupsByDate().LastOrDefault();

                if (entry == null)
                    return DateTime.Today;

                return (DateTime)entry.GroupTitle;
            }
        }

        private DateTime _userSelectedDate;
        public DateTime UserSelectedDate
        {
            get
            {
                if (_userSelectedDate == DateTime.MinValue)
                    _userSelectedDate = this.FirstCinemaDate;

                return _userSelectedDate;
            }
            set
            {
                this._userSelectedDate = value;

                this.RaisePropertyChanged("UserSelectedDate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
