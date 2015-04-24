using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Preferences;
using Android.Content;

namespace Cineworld
{
    public class Config
    {
        public enum RegionDef
        {
            UK,
            Ireland,
        }

        public static bool ShowSettings = false;
		public const string UseBrowserTag = "usebrowser";
        public const string RegionTag = "region";
        public const string FavCinemasTag = "favcinemas";
        public const string UserNameTag	 = "user";

		public const string AppSettings = "appsettings";

		private static Context _Context;

		public static void Initialise(Context ctx)
		{
			_Context = ctx;
		}
  
		public static RegionDef Region
		{
			get
			{
				return (RegionDef)(int)PreferenceManager.GetDefaultSharedPreferences(_Context).GetInt( RegionTag, 0);
			}
			set
			{
				var prefs = PreferenceManager.GetDefaultSharedPreferences (_Context);
				var prefEdit = prefs.Edit ();
				prefEdit.PutInt(RegionTag, (int)value);
				prefEdit.Apply ();
			}
		}

        public static bool UseBrowser
        {
            get
            {
				return PreferenceManager.GetDefaultSharedPreferences(_Context).GetBoolean(UseBrowserTag, false);
            }

            set
            {
				var prefs = PreferenceManager.GetDefaultSharedPreferences (_Context);
				var prefEdit = prefs.Edit ();
				prefEdit.PutBoolean(UseBrowserTag, value);
				prefEdit.Apply ();
            }
        }

		static char[] sepArray = { ',' };
        public static List<int> FavCinemas
        {
            get
            {
				List<int> cinemas = new List<int>();

				var favlist = PreferenceManager.GetDefaultSharedPreferences(_Context).GetString (FavCinemasTag, String.Empty).Split(sepArray, StringSplitOptions.RemoveEmptyEntries);
				foreach (var fav in favlist) 
				{
					cinemas.Add (Int32.Parse(fav));
				}

                return cinemas;
            }

            set
            {
				List<String> favStrings = new List<string> (value.Count);
				foreach (var fav in value) 
				{
					favStrings.Add (fav.ToString ());
				}

				string list = String.Join (",", favStrings);

				var prefs = PreferenceManager.GetDefaultSharedPreferences (_Context);
				var prefEdit = prefs.Edit ();
				prefEdit.PutString(FavCinemasTag, list);
				prefEdit.Apply ();
            }
        }

        public static string UserGuid
        {
            get
            {
				Guid guid = Guid.NewGuid ();

				return guid.ToString ("D");
            }

        }

        public static string UserName
        {
            get
            {
				return PreferenceManager.GetDefaultSharedPreferences(_Context).GetString(UserNameTag, null);
            }

            set
            {
				var prefs = PreferenceManager.GetDefaultSharedPreferences (_Context);
				var prefEdit = prefs.Edit ();
				prefEdit.PutString(UserNameTag, value);
				prefEdit.Apply ();
            }
        }
    }
}
