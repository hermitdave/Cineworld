using CineWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cineworld
{
    public class CinemaData
    {
        public CinemaData(Dictionary<int, CinemaInfo> cinemas)
        {
            foreach (var cin in cinemas)
                _Collection.Add(cin.Value);
        }

        public CinemaData(List<CinemaInfo> cinemas)
        {
            foreach (var cin in cinemas)
                _Collection.Add(cin);
        }

        private List<CinemaInfo> _Collection = new List<CinemaInfo>();

        public List<CinemaInfo> Collection
        {
            get
            {
                return this._Collection;
            }
        }

        internal IEnumerable<Group<CinemaInfo>> GetGroupsByLetter()
        {
            var emptyGroups = new List<Group<CinemaInfo>>()
            {
                new Group<CinemaInfo>("#", new List<CinemaInfo>()),
                new Group<CinemaInfo>("A", new List<CinemaInfo>()),
                new Group<CinemaInfo>("B", new List<CinemaInfo>()),
                new Group<CinemaInfo>("C", new List<CinemaInfo>()),
                new Group<CinemaInfo>("D", new List<CinemaInfo>()),
                new Group<CinemaInfo>("E", new List<CinemaInfo>()),
                new Group<CinemaInfo>("F", new List<CinemaInfo>()),
                new Group<CinemaInfo>("G", new List<CinemaInfo>()),
                new Group<CinemaInfo>("H", new List<CinemaInfo>()),
                new Group<CinemaInfo>("I", new List<CinemaInfo>()),
                new Group<CinemaInfo>("J", new List<CinemaInfo>()),
                new Group<CinemaInfo>("K", new List<CinemaInfo>()),
                new Group<CinemaInfo>("L", new List<CinemaInfo>()),
                new Group<CinemaInfo>("M", new List<CinemaInfo>()),
                new Group<CinemaInfo>("N", new List<CinemaInfo>()),
                new Group<CinemaInfo>("O", new List<CinemaInfo>()),
                new Group<CinemaInfo>("P", new List<CinemaInfo>()),
                new Group<CinemaInfo>("Q", new List<CinemaInfo>()),
                new Group<CinemaInfo>("R", new List<CinemaInfo>()),
                new Group<CinemaInfo>("S", new List<CinemaInfo>()),
                new Group<CinemaInfo>("T", new List<CinemaInfo>()),
                new Group<CinemaInfo>("U", new List<CinemaInfo>()),
                new Group<CinemaInfo>("V", new List<CinemaInfo>()),
                new Group<CinemaInfo>("W", new List<CinemaInfo>()),
                new Group<CinemaInfo>("X", new List<CinemaInfo>()),
                new Group<CinemaInfo>("Y", new List<CinemaInfo>()),
                new Group<CinemaInfo>("Z", new List<CinemaInfo>()),
            };

            var groupedCinemas = from cinema in this.Collection
                                group cinema by cinema.Region into c
                                orderby c.Key
                                select new Group<CinemaInfo>(c.Key, c);

            return groupedCinemas.Union(emptyGroups).OrderBy(g => g.Title);
        }
    }
}
