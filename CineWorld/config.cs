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
        static IsolatedStorageSettings _Settings = null;

        static IsolatedStorageSettings Settings
        {
            get
            {
                if (_Settings == null)
                {
                    _Settings = IsolatedStorageSettings.ApplicationSettings;
                }

                return _Settings;
            }
        }

        public static void Initialise()
        {
            var s = Settings;

            ShowSettings = false;

            bool bDelete = false;
            string version = String.Empty;
            
            if (!Settings.TryGetValue(VersionTag, out version))
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
                Settings[VersionTag] = NewAppVersion;
                Settings.Save();
            }

            var r = Region;
        }

        public static ApplicationTheme Theme
        {
            get
            {
                ApplicationTheme t = ApplicationTheme.Light;

                if (!Settings.Contains(ThemeTag))
                {
                    Settings.Add(ThemeTag, (int)t);
                    Settings.Save();
                }
                else
                    t = (ApplicationTheme)((int)Settings[ThemeTag]);

                return t;
            }

            set
            {
                Settings[ThemeTag] = (int)value;
                Settings.Save();
            }
        }

        public static bool AudioSupport
        {
            get
            {
                bool b = false;

                if (!Settings.Contains(AudioSupportTag))
                {
                    Settings.Add(AudioSupportTag, false);
                    Settings.Save();
                }
                else
                    b = (bool)(Settings[AudioSupportTag]);

                return b;
            }

            set
            {
                Settings[AudioSupportTag] = value;
                Settings.Save();
            }
        }

        public static bool ShowCleanBackground
        {
            get
            {
                bool b = false;

                if (!Settings.Contains(CleanBackgroundTag))
                {
                    Settings.Add(CleanBackgroundTag, false);
                    Settings.Save();
                }
                else
                    b = (bool)(Settings[CleanBackgroundTag]);

                return b;
            }

            set
            {
                Settings[CleanBackgroundTag] = value;
                Settings.Save();
            }
        }

        public static bool? AllowNokiaMusicSearch
        {
            get
            {
                bool? b = null;

                if (!Settings.Contains(AllowNokiaMusicTag))
                {
                    Settings.Add(AllowNokiaMusicTag, null);
                    Settings.Save();
                }
                else
                    b = (bool?)(Settings[AllowNokiaMusicTag]);

                return b;
            }

            set
            {
                Settings[AllowNokiaMusicTag] = value;
                Settings.Save();
            }
        }

        public static RegionDef Region
        {
            get
            {
                RegionDef d = RegionDef.UK;

                if (!Settings.Contains(RegionTag))
                {
                    Settings.Add(RegionTag, (int)d);
                    Settings.Save();
                }
                else
                    d = (RegionDef)((int)Settings[RegionTag]);

                return d;
            }

            set
            {
                Settings[RegionTag] = (int)value;
                Settings.Save();
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

                if (!Settings.Contains(UseLocationTag))
                {
                    Settings.Add(UseLocationTag, true);
                    Settings.Save();
                }
                else
                    b = (bool)(Settings[UseLocationTag]);

                return b;
            }

            set
            {
                Settings[UseLocationTag] = value;
                Settings.Save();
            }
        }

        public static bool UseMobileWeb
        {
            get
            {
                bool b = true;

                if (!Settings.Contains(UseMobileWebTag))
                {
                    Settings.Add(UseMobileWebTag, true);
                    Settings.Save();
                }
                else
                    b = (bool)(Settings[UseMobileWebTag]);

                return b;
            }

            set
            {
                Settings[UseMobileWebTag] = value;
                Settings.Save();
            }
        }

        public static bool AnimateLockscreen
        {
            get
            {
                bool b = true;

                if (!Settings.Contains(AnimateLockscreenTag))
                {
                    Settings.Add(AnimateLockscreenTag, true);
                    Settings.Save();
                }
                else
                    b = (bool)(Settings[AnimateLockscreenTag]);

                return b;
            }

            set
            {
                Settings[AnimateLockscreenTag] = value;
                Settings.Save();
            }
        }

        public static string CurrentLockscreen
        {
            get
            {
                string s = null;

                if (!Settings.Contains(CurrentLockscreenTag))
                {
                    Settings.Add(CurrentLockscreenTag, String.Empty);
                    Settings.Save();
                    s = String.Empty;
                }
                else
                    s = (string)(Settings[CurrentLockscreenTag]);

                return s;
            }

            set
            {
                Settings[CurrentLockscreenTag] = value;
                Settings.Save();
            }
        }

        public static bool AnimateTiles
        {
            get
            {
                bool b = true;

                if (!Settings.Contains(AnimateTilesTag))
                {
                    Settings.Add(AnimateTilesTag, true);
                    Settings.Save();
                }
                else
                    b = (bool)(Settings[AnimateTilesTag]);

                return b;
            }

            set
            {
                Settings[AnimateTilesTag] = value;
                Settings.Save();
            }
        }

        public static List<int> FilmPostersToIgnore
        {
            get
            {
                List<int> films = null;

                if (!Settings.Contains(FilmPostersToIgnoreTag))
                {
                    films = new List<int>();
                    Settings.Add(FilmPostersToIgnoreTag, films);
                    Settings.Save();
                }
                else
                    films = (List<int>)Settings[FilmPostersToIgnoreTag];

                return films;
            }

            set
            {
                Settings[FilmPostersToIgnoreTag] = value;
                Settings.Save();
            }
        }

        public static List<int> FavCinemas
        {
            get
            {
                List<int> cinemas = null;

                if (!Settings.Contains(FavCinemasTag))
                {
                    cinemas = new List<int>();
                    Settings.Add(FavCinemasTag, cinemas);
                    Settings.Save();
                }
                else
                    cinemas = (List<int>)Settings[FavCinemasTag];

                return cinemas;
            }

            set
            {
                Settings[FavCinemasTag] = value;
                Settings.Save();
            }
        }

        public static string UserGuid
        {
            get
            {
                string guid = null;

                if (!Settings.Contains(UserGuidTag))
                {
                    guid = Guid.NewGuid().ToString("D");
                    Settings.Add(UserGuidTag, guid);
                    Settings.Save();
                }
                else
                    guid = (string)Settings[UserGuidTag];

                return guid;
            }

            set
            {
                Settings[FavCinemasTag] = value;
                Settings.Save();
            }
        }

        public static string UserName
        {
            get
            {
                string name = null;

                if (!Settings.Contains(UserNameTag))
                    name = String.Empty;
                else
                    name = (string)Settings[UserNameTag];

                return name;
            }

            set
            {
                Settings[UserNameTag] = value;
                Settings.Save();
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
