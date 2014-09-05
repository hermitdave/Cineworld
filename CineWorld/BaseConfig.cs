using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
using CineWorld;
using System.Reflection;
#else
using Windows.Storage;
#endif

namespace Cineworld
{
    public enum RegionDef
    {
        UK,
        Ireland,
    }

    public class BaseConfig
    {
        
        private static bool ShowSettings = false;
        private const string RegionTag = "region";
        private const string QualityTag = "quality";
        private const string FavCinemasTag = "favcinemas";
        private const string UseLocationTag = "uselocation";
        private const string AnimateLockscreenTag = "lockscreenposters";
        private const string AnimateTilesTag = "tileposters";
        private const string VersionTag = "version";
        private const string NewAppVersion = "2.0.0.2";

        public static bool ShowRegion { get; set; }

        static ApplicationDataContainer settings = null;
        
        public static void Initialise()
        {
            settings = ApplicationData.Current.LocalSettings;

            var r = Region;
        }

        public static RegionDef Region
        {
            get
            {
                RegionDef d = RegionDef.UK;

                if (!settings.Values.ContainsKey(RegionTag))
                    settings.Values.Add(RegionTag, (int)d);
                else
                    d = (RegionDef)((int)settings.Values[RegionTag]);

                return d;
            }

            set
            {
                settings.Values[RegionTag] = (int)value;
            }
        }
    }
}
