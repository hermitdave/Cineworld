using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
using Cineworld;
using System.Reflection;
#else
using Windows.Storage;
using Windows.UI.Xaml;
#endif

namespace Cineworld
{
    public class Config
    {
#if WINDOWS_PHONE
        public enum ApplicationTheme
        {
            Dark,
            Light,
        }
#endif

        public enum RegionDef
        {
            UK,
            Ireland,
        }

#if !WIN8
        private static Version TargetedVersion = new Version(7, 10, 8858);
        public static bool IsTargetedVersion { get { return Environment.OSVersion.Version >= TargetedVersion; } }
#endif
        public static bool ShowSettings = false;
        public const string UseMobileWebTag = "usemobileweb";
        public const string RegionTag = "region";
        public const string QualityTag = "quality";
        public const string FavCinemasTag = "favcinemas";
        public const string UseLocationTag = "uselocation";
        public const string AnimateLockscreenTag = "lockscreenposters";
        public const string CurrentLockscreenTag = "currentlockscreen";
        public const string FilmPostersToIgnoreTag = "ignorefilms";
        public const string AnimateTilesTag = "tileposters";
        public const string UserGuidTag = "guid";
        public const string UserNameTag = "user";
        public const string VersionTag = "version";
        public const string NewAppVersion = "2.2.0.0";
        public const string ThemeTag = "theme";
        public const string AudioSupportTag = "audiosupport";
        public const string CleanBackgroundTag = "cleanbackground";
        public const string AllowNokiaMusicTag = "nokiamusicsearch";
        
#if WIN8
        public static event Action RegionChanged = delegate { };
#endif

        public static bool ShowRegion { get; set; }

#if WINDOWS_PHONE
        static IsolatedStorageSettings settings = null;

        public static void Initialise()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;

            ShowSettings = false;

            bool bDelete = false;
            string version = String.Empty;
            
            if (!settings.TryGetValue(VersionTag, out version))
                bDelete = ShowSettings = true;
            else
            {
                Version vPrevious = Version.Parse(version);
                Version vApp = Version.Parse(NewAppVersion);

                if (vApp > vPrevious)
                    bDelete = true;
                else
                    bDelete = false;

                if (vApp > vPrevious)
                    ShowSettings = true;
            }

            if (bDelete && !Config.AnimateLockscreen)
            {
                BaseStorageHelper helper = new BaseStorageHelper();
                helper.CleanSharedContent();
            }

            if (ShowSettings)
            {
                settings[VersionTag] = NewAppVersion;
                settings.Save();
            }

            var r = Region;
        }

        public static ApplicationTheme Theme
        {
            get
            {
                ApplicationTheme t = ApplicationTheme.Light;

                if (!settings.Contains(ThemeTag))
                {
                    settings.Add(ThemeTag, (int)t);
                    settings.Save();
                }
                else
                    t = (ApplicationTheme)((int)settings[ThemeTag]);

                return t;
            }

            set
            {
                settings[ThemeTag] = (int)value;
                settings.Save();
            }
        }

        public static bool AudioSupport
        {
            get
            {
                bool b = false;

                if (!settings.Contains(AudioSupportTag))
                {
                    settings.Add(AudioSupportTag, false);
                    settings.Save();
                }
                else
                    b = (bool)(settings[AudioSupportTag]);

                return b;
            }

            set
            {
                settings[AudioSupportTag] = value;
                settings.Save();
            }
        }

        public static bool ShowCleanBackground
        {
            get
            {
                bool b = false;

                if (!settings.Contains(CleanBackgroundTag))
                {
                    settings.Add(CleanBackgroundTag, false);
                    settings.Save();
                }
                else
                    b = (bool)(settings[CleanBackgroundTag]);

                return b;
            }

            set
            {
                settings[CleanBackgroundTag] = value;
                settings.Save();
            }
        }

        public static bool? AllowNokiaMusicSearch
        {
            get
            {
                bool? b = null;

                if (!settings.Contains(AllowNokiaMusicTag))
                {
                    settings.Add(AllowNokiaMusicTag, null);
                    settings.Save();
                }
                else
                    b = (bool?)(settings[AllowNokiaMusicTag]);

                return b;
            }

            set
            {
                settings[AllowNokiaMusicTag] = value;
                settings.Save();
            }
        }

        public static RegionDef Region
        {
            get
            {
                RegionDef d = RegionDef.UK;

                if (!settings.Contains(RegionTag))
                {
                    settings.Add(RegionTag, (int)d);
                    settings.Save();
                }
                else
                    d = (RegionDef)((int)settings[RegionTag]);

                return d;
            }

            set
            {
                settings[RegionTag] = (int)value;
                settings.Save();
#if !WINDOWS_PHONE
                if (RegionChanged != null)
                    RegionChanged();
#endif
            }
        }

        public static bool UseLocation
        {
            get
            {
                bool b = true;

                if (!settings.Contains(UseLocationTag))
                {
                    settings.Add(UseLocationTag, true);
                    settings.Save();
                }
                else
                    b = (bool)(settings[UseLocationTag]);

                return b;
            }

            set
            {
                settings[UseLocationTag] = value;
                settings.Save();
            }
        }

        public static bool UseMobileWeb
        {
            get
            {
                bool b = true;

                if (!settings.Contains(UseMobileWebTag))
                {
                    settings.Add(UseMobileWebTag, true);
                    settings.Save();
                }
                else
                    b = (bool)(settings[UseMobileWebTag]);

                return b;
            }

            set
            {
                settings[UseMobileWebTag] = value;
                settings.Save();
            }
        }

        public static bool AnimateLockscreen
        {
            get
            {
                bool b = true;

                if (!settings.Contains(AnimateLockscreenTag))
                {
                    settings.Add(AnimateLockscreenTag, true);
                    settings.Save();
                }
                else
                    b = (bool)(settings[AnimateLockscreenTag]);

                return b;
            }

            set
            {
                settings[AnimateLockscreenTag] = value;
                settings.Save();
            }
        }

        public static string CurrentLockscreen
        {
            get
            {
                string s = null;

                if (!settings.Contains(CurrentLockscreenTag))
                {
                    settings.Add(CurrentLockscreenTag, String.Empty);
                    settings.Save();
                    s = String.Empty;
                }
                else
                    s = (string)(settings[CurrentLockscreenTag]);

                return s;
            }

            set
            {
                settings[CurrentLockscreenTag] = value;
                settings.Save();
            }
        }

        public static bool AnimateTiles
        {
            get
            {
                bool b = true;

                if (!settings.Contains(AnimateTilesTag))
                {
                    settings.Add(AnimateTilesTag, true);
                    settings.Save();
                }
                else
                    b = (bool)(settings[AnimateTilesTag]);

                return b;
            }

            set
            {
                settings[AnimateTilesTag] = value;
                settings.Save();
            }
        }

        public static List<int> FilmPostersToIgnore
        {
            get
            {
                List<int> films = null;

                if (!settings.Contains(FilmPostersToIgnoreTag))
                {
                    films = new List<int>();
                    settings.Add(FilmPostersToIgnoreTag, films);
                    settings.Save();
                }
                else
                    films = (List<int>)settings[FilmPostersToIgnoreTag];

                return films;
            }

            set
            {
                settings[FilmPostersToIgnoreTag] = value;
                settings.Save();
            }
        }

        public static List<int> FavCinemas
        {
            get
            {
                List<int> cinemas = null;

                if (!settings.Contains(FavCinemasTag))
                {
                    cinemas = new List<int>();
                    settings.Add(FavCinemasTag, cinemas);
                    settings.Save();
                }
                else
                    cinemas = (List<int>)settings[FavCinemasTag];

                return cinemas;
            }

            set
            {
                settings[FavCinemasTag] = value;
                settings.Save();
            }
        }

        public static string UserGuid
        {
            get
            {
                string guid = null;

                if (!settings.Contains(UserGuidTag))
                {
                    guid = Guid.NewGuid().ToString("D");
                    settings.Add(UserGuidTag, guid);
                    settings.Save();
                }
                else
                    guid = (string)settings[UserGuidTag];

                return guid;
            }

            set
            {
                settings[FavCinemasTag] = value;
                settings.Save();
            }
        }

        public static string UserName
        {
            get
            {
                string name = null;

                if (!settings.Contains(UserNameTag))
                    name = String.Empty;
                else
                    name = (string)settings[UserNameTag];

                return name;
            }

            set
            {
                settings[UserNameTag] = value;
                settings.Save();
            }
        }
#else
        static ApplicationDataContainer settings = null;
        
        public static void Initialise()
        {
            settings = ApplicationData.Current.LocalSettings;

            ShowSettings = false;

            bool bDelete = false;
            string version = String.Empty;

            if (!settings.Values.ContainsKey(VersionTag))
                bDelete = ShowSettings = true;
            else
            {
                version = (string)settings.Values[VersionTag];
                Version vPrevious = Version.Parse(version);
                Version vApp = Version.Parse(NewAppVersion);

                if (vApp > vPrevious)
                    ShowSettings = true;
            }

            if (bDelete)
            {
                BaseStorageHelper helper = new BaseStorageHelper();
                helper.CleanSharedContent();
            }

            if (ShowSettings)
            {
                settings.Values[VersionTag] = NewAppVersion;
            }

            var r = Region;
            var q = TrailerQuality;
            var w = UseMobileWeb;
        }

        public static ApplicationTheme Theme
        {
            get
            {
                ApplicationTheme t = ApplicationTheme.Light;

                if (!settings.Values.ContainsKey(ThemeTag))
                    settings.Values.Add(ThemeTag, (int)t);
                else
                    t = (ApplicationTheme)((int)settings.Values[ThemeTag]);

                return t;
            }

            set
            {
                settings.Values[ThemeTag] = (int)value;
            }
        }

        public static bool ShowCleanBackground
        {
            get
            {
                bool b = false;

                if (!settings.Values.ContainsKey(CleanBackgroundTag))
                    settings.Values.Add(CleanBackgroundTag, false);
                else
                    b = (bool)(settings.Values[CleanBackgroundTag]);

                return b;
            }

            set
            {
                settings.Values[CleanBackgroundTag] = value;
            }
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
#if WIN8
                if (RegionChanged != null)
                    RegionChanged();
#endif
            }
        }

        public static bool UseMobileWeb
        {
            get
            {
                bool b = true;

                if (!settings.Values.ContainsKey(UseMobileWebTag))
                    settings.Values.Add(UseMobileWebTag, true);
                else
                    b = (bool)(settings.Values[UseMobileWebTag]);

                return b;
            }

            set
            {
                settings.Values[UseMobileWebTag] = value;
            }
        }

        public static bool UseLocation
        {
            get
            {
                bool b = true;

                if (!settings.Values.ContainsKey(UseLocationTag))
                    settings.Values.Add(UseLocationTag, true);
                else
                    b = (bool)(settings.Values[UseLocationTag]);

                return b;
            }

            set
            {
                settings.Values[UseLocationTag] = value;
            }
        }

        

        public static List<int> FavCinemas
        {
            get
            {
                List<int> cinemas = null;

                if (!settings.Values.ContainsKey(FavCinemasTag))
                {
                    cinemas = new List<int>();
                    //settings.Values.Add(FavCinemasTag, cinemas);
                    PersistHelper.SaveSettingToStorage(FavCinemasTag, cinemas, settings);
                }
                else
                    cinemas = PersistHelper.LoadSetttingFromStorage<List<int>>(FavCinemasTag, settings); //(List<int>)settings.Values[FavCinemasTag];

                return cinemas;
            }

            set
            {
                //settings.Values[FavCinemasTag] = value;
                PersistHelper.SaveSettingToStorage(FavCinemasTag, value, settings);
            }
        }

        public static string UserGuid
        {
            get
            {
                string guid = null;

                if (!settings.Values.ContainsKey(UserGuidTag))
                {
                    guid = Guid.NewGuid().ToString("D");
                    settings.Values.Add(UserGuidTag, guid);
                }
                else
                    guid = (string)settings.Values[UserGuidTag];

                return guid;
            }

            set
            {
                settings.Values[FavCinemasTag] = value;
            }
        }

        public static string UserName
        {
            get
            {
                string name = null;

                if (!settings.Values.ContainsKey(UserNameTag))
                    name = String.Empty;
                else
                    name = (string)settings.Values[UserNameTag];

                return name;
            }

            set
            {
                settings.Values[UserNameTag] = value;
            }
        }

        public static MyToolkit.Multimedia.YouTubeQuality TrailerQuality
        {
            get
            {
                int q = 0;

                if (!settings.Values.ContainsKey(QualityTag))
                    settings.Values.Add(QualityTag, (int)q);
                else
                    q = (int)settings.Values[QualityTag];

                return GetYoutubeQualityFromInt(q);
            }

            set
            {
                settings.Values[QualityTag] = GetIntFromYoutubeQuality(value);
            }
        }

        public static MyToolkit.Multimedia.YouTubeQuality GetYoutubeQualityFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return MyToolkit.Multimedia.YouTubeQuality.Quality480P;
                    
                case 1:
                    return MyToolkit.Multimedia.YouTubeQuality.Quality720P;
                    
                default:
                    return MyToolkit.Multimedia.YouTubeQuality.Quality1080P;
            }
        }

        public static int GetIntFromYoutubeQuality(MyToolkit.Multimedia.YouTubeQuality e)
        {
            switch (e)
            {
                case MyToolkit.Multimedia.YouTubeQuality.Quality480P:
                    return 0;

                case MyToolkit.Multimedia.YouTubeQuality.Quality720P:
                    return 1;

                default:
                    return 2;
            }
        }
#endif
    }
}
