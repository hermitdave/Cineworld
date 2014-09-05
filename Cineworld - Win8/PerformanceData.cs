using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cineworld
{
    public class PerformanceData
    {
        public PerformanceData(List<PerformanceInfo> performances)
        {
            this.Collection = new ItemCollection<PerformanceInfo>();

            foreach (var perf in performances)
                Collection.Add(perf);
        }

        private ItemCollection<PerformanceInfo> Collection { get; set; }

        private List<GroupInfoList<object>> groupsByDate = null;

        public List<GroupInfoList<object>> GroupsByDate
        {
            get
            {
                if (groupsByDate == null)
                {
                    groupsByDate = new List<GroupInfoList<object>>();

                    var query = from item in this.Collection
                                 orderby ((PerformanceInfo)item).PerformanceTS
                                 group item by ((PerformanceInfo)item).PerformanceTS.Date into g
                                 select new { GroupName = g.Key, Items = g };

                    foreach (var g in query)
                    {
                        GroupInfoList<object> info = new GroupInfoList<object>();
                        info.Key = g.GroupName.ToString("ddd, dd MMM yyyy");
                        foreach (var item in g.Items)
                        {
                            info.Add(item);
                        }
                        groupsByDate.Add(info);
                    }
                }

                return groupsByDate;
            }
        }

        List<DateHeaderItem> performanceHeaders = null;
        public List<DateHeaderItem> PerformanceHeaders
        {
            get
            {
                if (performanceHeaders == null)
                {
                    performanceHeaders = new List<DateHeaderItem>();

                    foreach (var group in groupsByDate)
                    {
                        performanceHeaders.Add(new DateHeaderItem() { Name = (string)group.Key, IsEnabled = true, Group = group });
                    }
                }

                return performanceHeaders;
            }
        }
    }
}
