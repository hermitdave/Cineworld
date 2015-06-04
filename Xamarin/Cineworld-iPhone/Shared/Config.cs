using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;

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
  
		public static RegionDef Region
		{
			get
			{
				return (RegionDef)(int)NSUserDefaults.StandardUserDefaults.IntForKey (RegionTag);
			}
			set
			{
				NSUserDefaults.StandardUserDefaults.SetInt((nint)(int)value, RegionTag);
				NSUserDefaults.StandardUserDefaults.Synchronize ();
			}
		}

        public static bool UseBrowser
        {
            get
            {
				return NSUserDefaults.StandardUserDefaults.BoolForKey(UseBrowserTag);
            }

            set
            {
				NSUserDefaults.StandardUserDefaults.SetBool (value, UseBrowserTag);
				NSUserDefaults.StandardUserDefaults.Synchronize ();
            }
        }

		static char[] sepArray = { ',' };
        public static List<int> FavCinemas
        {
            get
            {
				List<int> cinemas = new List<int>();

				var favlist = NSUserDefaults.StandardUserDefaults.StringForKey (FavCinemasTag).Split(sepArray, StringSplitOptions.RemoveEmptyEntries);
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

				NSUserDefaults.StandardUserDefaults.SetString(list, FavCinemasTag);
				NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static string UserGuid
        {
            get
            {
				UIDevice device = new UIDevice ();

				return device.IdentifierForVendor.AsString ();
            }

        }

        public static string UserName
        {
            get
            {
				return NSUserDefaults.StandardUserDefaults.StringForKey(UserNameTag);
            }

            set
            {
				NSUserDefaults.StandardUserDefaults.SetString (value, UserNameTag);
				NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }
    }
}
