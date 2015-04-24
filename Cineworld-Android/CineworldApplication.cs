using System;
using Cineworld;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Android.Locations;

namespace Cineworld_Android
{
	public static class CineworldApplication
	{
		static CineworldApplication ()
		{
		}

		public static Configuration TMDBConfig { get; set; }

		public static bool AdFree { get; set; }

		public static List<string> Posters { get; set; }

		public static Dictionary<int, FilmInfo> Films { get; set; }

		public static Dictionary<int, CinemaInfo> Cinemas { get; set; }

		public static Dictionary<int, List<FilmInfo>> CinemaFilms { get; set; }

		public static Dictionary<int, List<CinemaInfo>> FilmCinemas { get; set; }

		public static MobileServiceClient MobileService { get; set; }

		public static Location UserLocation { get; set; }

		public static string DataDownloadsDir { get; set; }
	}
}

