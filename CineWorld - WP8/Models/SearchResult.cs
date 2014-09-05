using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineWorld.Models
{
    public class SearchResult
    {
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public Uri Image { get; set; }
        public object SearchObject { get; set; }
    }
}
