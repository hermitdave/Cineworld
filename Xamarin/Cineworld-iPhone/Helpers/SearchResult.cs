using System;

namespace CineworldiPhone
{
	public class SearchResult
	{
		public SearchResult ()
		{
		}

		public string Name { get; set; }
		public string Subtitle { get; set; }
		public Uri Image { get; set; }
		public object SearchObject { get; set; }
	}
}

