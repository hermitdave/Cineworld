using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Cineworld;
using Microsoft.WindowsAzure.MobileServices;
using CoreLocation;

namespace CineworldiPhone
{
	public class Application
	{
		public static Configuration TMDBConfig { get; set; }

		public static List<string> Posters { get; set; }

		public static Dictionary<int, FilmInfo> Films { get; set; }

		public static Dictionary<int, CinemaInfo> Cinemas { get; set; }

		public static Dictionary<int, List<FilmInfo>> CinemaFilms { get; set; }

		public static Dictionary<int, List<CinemaInfo>> FilmCinemas { get; set; }

		public static MobileServiceClient MobileService { get; set; }

		public static CLLocation UserLocation { get; set; }

		public static string DataDownloadsDir { get; set; }

		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.

			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
