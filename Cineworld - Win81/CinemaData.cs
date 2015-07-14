using System;
using System.Collections.Generic;
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

        private ItemCollection<CinemaInfo> _Collection = new ItemCollection<CinemaInfo>();

        public ItemCollection<CinemaInfo> Collection
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
                                orderby ((CinemaInfo)item).Name
                                group item by ((CinemaInfo)item).Name[0] into g
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

        List<HeaderItem> cinemaHeaders = null;
        public List<HeaderItem> CinemaHeaders
        {
            get
            {
                if (cinemaHeaders == null)
                {
                    cinemaHeaders = new List<HeaderItem>();

                    for (int i = 65; i <= 90; i++)
                    {
                        char c = (char)i;

                        if (this.GroupsByLetter.Exists(k => ((char)k.Key) == c))
                            cinemaHeaders.Add(new HeaderItem() { Name = c, IsEnabled = true });
                        else
                            cinemaHeaders.Add(new HeaderItem() { Name = c, IsEnabled = false });
                    }

                    // insert # for numbers at the front
                    cinemaHeaders.Insert(0, new HeaderItem() { Name = '#', IsEnabled = false });
                }
                
                return cinemaHeaders;
            }
        }
    }
}
