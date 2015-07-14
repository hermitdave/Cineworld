using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cineworld
{
    public class HeaderItem
    {
        public char Name { get; set; }
        public bool IsEnabled { get; set; }

        public GroupInfoList<object> Group { get; set; }
    }

    public class DateHeaderItem
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public GroupInfoList<object> Group { get; set; }
    }
}
