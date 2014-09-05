using CineWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cineworld
{
    public class FilmData
    {
        public FilmData(Dictionary<int, FilmInfo> films)
        {
            foreach (var film in films)
                _Collection.Add(film.Value);
        }

        public FilmData(List<FilmInfo> films)
        {
            foreach (var film in films)
                _Collection.Add(film);
        }

        private List<FilmInfo> _Collection = new List<FilmInfo>();

        public List<FilmInfo> Collection
        {
            get
            {
                return this._Collection;
            }
        }

        internal IEnumerable<Group<FilmInfo>> GetGroupsByLetter()
        {
            var emptyGroups = new List<Group<FilmInfo>>()
            {
                new Group<FilmInfo>("#", new List<FilmInfo>()),
                new Group<FilmInfo>("A", new List<FilmInfo>()),
                new Group<FilmInfo>("B", new List<FilmInfo>()),
                new Group<FilmInfo>("C", new List<FilmInfo>()),
                new Group<FilmInfo>("D", new List<FilmInfo>()),
                new Group<FilmInfo>("E", new List<FilmInfo>()),
                new Group<FilmInfo>("F", new List<FilmInfo>()),
                new Group<FilmInfo>("G", new List<FilmInfo>()),
                new Group<FilmInfo>("H", new List<FilmInfo>()),
                new Group<FilmInfo>("I", new List<FilmInfo>()),
                new Group<FilmInfo>("J", new List<FilmInfo>()),
                new Group<FilmInfo>("K", new List<FilmInfo>()),
                new Group<FilmInfo>("L", new List<FilmInfo>()),
                new Group<FilmInfo>("M", new List<FilmInfo>()),
                new Group<FilmInfo>("N", new List<FilmInfo>()),
                new Group<FilmInfo>("O", new List<FilmInfo>()),
                new Group<FilmInfo>("P", new List<FilmInfo>()),
                new Group<FilmInfo>("Q", new List<FilmInfo>()),
                new Group<FilmInfo>("R", new List<FilmInfo>()),
                new Group<FilmInfo>("S", new List<FilmInfo>()),
                new Group<FilmInfo>("T", new List<FilmInfo>()),
                new Group<FilmInfo>("U", new List<FilmInfo>()),
                new Group<FilmInfo>("V", new List<FilmInfo>()),
                new Group<FilmInfo>("W", new List<FilmInfo>()),
                new Group<FilmInfo>("X", new List<FilmInfo>()),
                new Group<FilmInfo>("Y", new List<FilmInfo>()),
                new Group<FilmInfo>("Z", new List<FilmInfo>()),
            };
            var cinemasByChar = from film in this.Collection
                                group film by film.HeaderChar.ToString() into c
                                orderby c.Key
                                select new Group<FilmInfo>(c.Key, c);

            return cinemasByChar.Union(emptyGroups).OrderBy(g => g.Title);
        }

        internal List<Group<FilmInfo>> GetGroupsByDate()
        {
            Dictionary<DateTime, Dictionary<int, FilmInfo>> dateFilms = new Dictionary<DateTime, Dictionary<int, FilmInfo>>();
            foreach (var film in this._Collection)
            {
                foreach (var pi in film.Performances)
                {
                    Dictionary<int, FilmInfo> filmList = null;
                    if (!dateFilms.TryGetValue(pi.PerformanceTS.Date, out filmList))
                    {
                        filmList = new Dictionary<int, FilmInfo>();
                        dateFilms.Add(pi.PerformanceTS.Date, filmList);
                    }

                    if (!filmList.ContainsKey(film.EDI))
                    {
                        var f = film.Clone();
                        f.Performances = new List<PerformanceInfo>(film.Performances.Where(p => p.PerformanceTS.Date == pi.PerformanceTS.Date));
                        filmList.Add(film.EDI, f);
                    }
                }
            }

            List<Group<FilmInfo>> retList = new List<Group<FilmInfo>>();
            List<DateTime> dates = dateFilms.Keys.ToList();
            dates.Sort();
            foreach (DateTime date in dates)
            {
                List<FilmInfo> filmsForDate = dateFilms[date].Values.OrderBy(f => f.Title).ToList();
                Group<FilmInfo> gFI = new Group<FilmInfo>(date.ToString("ddd, dd MMM yyy"), filmsForDate);
                retList.Add(gFI);
            }

            return retList;
        }

        Dictionary<DateTime, Dictionary<int, FilmInfo>> dateFilms = new Dictionary<DateTime, Dictionary<int, FilmInfo>>();

        internal IEnumerable<Group<FilmInfo>> GetGroupForDate(DateTime userSelected)
        {
            if (dateFilms.Count == 0)
            {
                foreach (var film in this._Collection)
                {
                    foreach (var pi in film.Performances)
                    {
                        Dictionary<int, FilmInfo> filmList = null;
                        if (!dateFilms.TryGetValue(pi.PerformanceTS.Date, out filmList))
                        {
                            filmList = new Dictionary<int, FilmInfo>();
                            dateFilms.Add(pi.PerformanceTS.Date, filmList);
                        }

                        if (!filmList.ContainsKey(film.EDI))
                        {
                            var f = film.Clone();
                            f.Performances = new List<PerformanceInfo>(film.Performances.Where(p => p.PerformanceTS.Date == pi.PerformanceTS.Date));
                            filmList.Add(film.EDI, f);
                        }
                    }
                }
            }

            if (!dateFilms.ContainsKey(userSelected))
                return new List<Group<FilmInfo>>();

            var emptyGroups = new List<Group<FilmInfo>>()
            {
                new Group<FilmInfo>("#", new List<FilmInfo>()),
                new Group<FilmInfo>("A", new List<FilmInfo>()),
                new Group<FilmInfo>("B", new List<FilmInfo>()),
                new Group<FilmInfo>("C", new List<FilmInfo>()),
                new Group<FilmInfo>("D", new List<FilmInfo>()),
                new Group<FilmInfo>("E", new List<FilmInfo>()),
                new Group<FilmInfo>("F", new List<FilmInfo>()),
                new Group<FilmInfo>("G", new List<FilmInfo>()),
                new Group<FilmInfo>("H", new List<FilmInfo>()),
                new Group<FilmInfo>("I", new List<FilmInfo>()),
                new Group<FilmInfo>("J", new List<FilmInfo>()),
                new Group<FilmInfo>("K", new List<FilmInfo>()),
                new Group<FilmInfo>("L", new List<FilmInfo>()),
                new Group<FilmInfo>("M", new List<FilmInfo>()),
                new Group<FilmInfo>("N", new List<FilmInfo>()),
                new Group<FilmInfo>("O", new List<FilmInfo>()),
                new Group<FilmInfo>("P", new List<FilmInfo>()),
                new Group<FilmInfo>("Q", new List<FilmInfo>()),
                new Group<FilmInfo>("R", new List<FilmInfo>()),
                new Group<FilmInfo>("S", new List<FilmInfo>()),
                new Group<FilmInfo>("T", new List<FilmInfo>()),
                new Group<FilmInfo>("U", new List<FilmInfo>()),
                new Group<FilmInfo>("V", new List<FilmInfo>()),
                new Group<FilmInfo>("W", new List<FilmInfo>()),
                new Group<FilmInfo>("X", new List<FilmInfo>()),
                new Group<FilmInfo>("Y", new List<FilmInfo>()),
                new Group<FilmInfo>("Z", new List<FilmInfo>()),
            };

            var cinemasByChar = from film in dateFilms[userSelected].Values
                                group film by film.HeaderChar.ToString() into c
                                orderby c.Key
                                select new Group<FilmInfo>(c.Key, c);

            return cinemasByChar.Union(emptyGroups).OrderBy(g => g.Title);
        }
    }
}
