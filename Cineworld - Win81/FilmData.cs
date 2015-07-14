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

        private ItemCollection<FilmInfo> _Collection = new ItemCollection<FilmInfo>();

        public ItemCollection<FilmInfo> Collection
        {
            get
            {
                return this._Collection;
            }
        }

        private List<GroupInfoList<object>> groupsByLetter = null;

        public List<GroupInfoList<object>> GroupsByLetter
        {
            get
            {
                if (groupsByLetter == null)
                {
                    groupsByLetter = new List<GroupInfoList<object>>();

                    var query = from item in Collection
                                orderby ((FilmInfo)item).Title
                                group item by ((FilmInfo)item).HeaderChar into g
                                select new { GroupName = g.Key, Items = g };

                    foreach (var g in query)
                    {
                        GroupInfoList<object> info = new GroupInfoList<object>();
                        info.Key = g.GroupName;
                        foreach (var item in g.Items)
                        {
                            info.Add(item);
                        }
                        groupsByLetter.Add(info);
                    }
                }

                return groupsByLetter;
            }
        }

        List<FilmInfo> current = null;
        List<FilmInfo> upcomig = null;

        public List<GroupInfoList<object>> CurrentFilmGroups
        {
            get
            {
                EnsureFilmDataSets();

                groupsByLetter = new List<GroupInfoList<object>>();

                var query = from item in current
                            orderby ((FilmInfo)item).Title
                            group item by ((FilmInfo)item).HeaderChar into g
                            select new { GroupName = g.Key, Items = g };

                foreach (var g in query)
                {
                    GroupInfoList<object> info = new GroupInfoList<object>();
                    info.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        info.Add(item);
                    }
                    groupsByLetter.Add(info);
                }

                return groupsByLetter;
            }
        }

        public List<GroupInfoList<object>> UpcomingFilmGroups
        {
            get
            {
                EnsureFilmDataSets();

                groupsByLetter = new List<GroupInfoList<object>>();

                var query = from item in upcomig
                            orderby ((FilmInfo)item).Title
                            group item by ((FilmInfo)item).HeaderChar into g
                            select new { GroupName = g.Key, Items = g };

                foreach (var g in query)
                {
                    GroupInfoList<object> info = new GroupInfoList<object>();
                    info.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        info.Add(item);
                    }
                    groupsByLetter.Add(info);
                }

                return groupsByLetter;
            }
        }

        private void EnsureFilmDataSets()
        {
            if (current == null && upcomig == null)
            {
                current = new List<FilmInfo>();
                upcomig = new List<FilmInfo>();

                foreach (FilmInfo film in this.Collection)
                {
                    if (film.Release <= DateTime.UtcNow)
                        current.Add(film);
                    else
                        upcomig.Add(film);
                }
            }
        }

        public List<GroupInfoList<object>> GroupsByDate
        {
            get
            {
                Dictionary<DateTime, Dictionary<int, FilmInfo>> dateFilms = new Dictionary<DateTime, Dictionary<int, FilmInfo>>();
                foreach (FilmInfo film in this._Collection)
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

                List<GroupInfoList<object>> retList = new List<GroupInfoList<object>>();
                List<DateTime> dates = dateFilms.Keys.ToList();
                dates.Sort();
                foreach (DateTime date in dates)
                {
                    GroupInfoList<object> info = new GroupInfoList<object>();
                    info.Key = date.ToString("ddd, dd MMM yyyy");
                    info.AddRange(dateFilms[date].Values.OrderBy(f => f.Title));
                    
                    retList.Add(info);
                }

                return retList;
            }
        }

        Dictionary<DateTime, Dictionary<int, FilmInfo>> dateFilms = new Dictionary<DateTime, Dictionary<int, FilmInfo>>();

        internal List<GroupInfoList<object>> GetGroupForDate(DateTime userSelected)
        {
            if (dateFilms.Count == 0)
            {
                foreach (FilmInfo film in this._Collection)
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
                return new List<GroupInfoList<object>>();

            List<GroupInfoList<object>> groupedData = new List<GroupInfoList<object>>();

            var query = from item in dateFilms[userSelected].Values
                        orderby ((FilmInfo)item).Title
                        group item by ((FilmInfo)item).HeaderChar into g
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                GroupInfoList<object> info = new GroupInfoList<object>();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                groupedData.Add(info);
            }
            
            return groupedData;
        }

        public List<HeaderItem> GetFilmHeaders(List<GroupInfoList<object>> groupedForDate)
        {
            List<HeaderItem> filmHeaders = new List<HeaderItem>();

            char c = '#';

            filmHeaders.Add(new HeaderItem() { Name = '#', IsEnabled = groupedForDate.Exists(k => ((char)k.Key) == c) });

            for (int i = 65; i <= 90; i++)
            {
                c = (char)i;

                GroupInfoList<object> grp = groupedForDate.FirstOrDefault(g => (char)g.Key == c);
                if (grp != null)
                    filmHeaders.Add(new HeaderItem() { Name = c, IsEnabled = true, Group = grp });
                else
                    filmHeaders.Add(new HeaderItem() { Name = c, IsEnabled = false });
            }

            return filmHeaders;
        }
    }
}
