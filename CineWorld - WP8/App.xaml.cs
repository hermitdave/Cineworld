using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Cineworld;
using Microsoft.Phone.Scheduler;
using Microsoft.WindowsAzure.MobileServices;
using System.Globalization;
using Nokia.Music;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using NokiaFeedbackDemo.Helpers;
using Microsoft.Phone.BackgroundAudio;
using Windows.ApplicationModel.Store;

namespace CineWorld
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static bool IsFree { get; set; }

        public static ListingInformation ListingInfo { get; set; }

        public const string AdFreeIAP = "AdFree";

        public static Configuration TMDBConfig { get; set; }

        public static List<string> Posters { get; set; }

        public static Dictionary<int, FilmInfo> Films { get; set; }

        public static Dictionary<int, CinemaInfo> Cinemas { get; set; }

        public static Dictionary<int, List<FilmInfo>> CinemaFilms { get; set; }

        public static Dictionary<int, List<CinemaInfo>> FilmCinemas { get; set; }

        public static bool AllowNokiaMusicSearch { get; set; }

        private bool _reset;

        //public static MobileServiceClient MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");

        public static MobileServiceClient MobileService { get; set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            Films = new Dictionary<int, FilmInfo>();
            Cinemas = new Dictionary<int, CinemaInfo>();
            CinemaFilms = new Dictionary<int, List<FilmInfo>>();
            FilmCinemas = new Dictionary<int, List<CinemaInfo>>();

            Config.Initialise();

            // Phone-specific initialization
            InitializePhoneApplication();

            ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.SystemTrayColors;
            
            if (Config.Theme == Config.ApplicationTheme.Light)
                ThemeManager.ToLightTheme();
            else
                ThemeManager.ToDarkTheme();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private async void Application_Launching(object sender, LaunchingEventArgs e)
        {
            if (CurrentApp.LicenseInformation != null
                && CurrentApp.LicenseInformation.ProductLicenses != null
                && CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(AdFreeIAP))
            {
                App.IsFree = !CurrentApp.LicenseInformation.ProductLicenses[AdFreeIAP].IsActive;
            }
            else
            {
                App.IsFree = true;
            }
            try
            {
                App.ListingInfo = await CurrentApp.LoadListingInformationAsync();
            }
            catch { }

            ApplicationUsageHelper.Init("2.2");
            
            await CheckNokiaMusicCountryCode();
            //CheckForUpdatedVersion();
            PromptForRating();

            this.StartPeriodicAgent();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            BackgroundAudioPlayer.Instance.Stop();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            BackgroundAudioPlayer.Instance.Stop();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

//        public static string GetManifestAttributeValue(string attributeName)
//        {
//            var xmlReaderSettings = new XmlReaderSettings
//            {
//                XmlResolver = new XmlXapResolver()
//            };

//            using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
//            {
//                xmlReader.ReadToDescendant("App");

//                return xmlReader.GetAttribute(attributeName);
//            }
//        }

//        private async void CheckForUpdatedVersion()
//        {
//            var currentVersion = new Version(GetManifestAttributeValue("Version"));

//            MarketplaceInformationService infoService = new MarketplaceInformationService();

//#if DEBUG
//            var info = await infoService.GetAppInformationAsync("9b255dee-4682-49d8-88bc-d568e40f84bc");
//#else
//            var info = await infoService.GetAppInformationAsync();
//#endif

//            var updatedVersion = new Version(info.Entry.Version);

//            if (updatedVersion > currentVersion
//                && MessageBox.Show("Do you want to install the new version now?", "Update Available", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
//            {
//                new MarketplaceDetailTask().Show();
//            }
//        }

        private void PromptForRating()
        {
            string REVIEWED = "REVIEWED";
            bool reviewed = StorageHelper.GetSetting<bool>(REVIEWED);

            if (!reviewed)
            {
                RadRateApplicationReminder rateAppReminder = new RadRateApplicationReminder()
                {
                    RecurrencePerUsageCount = 5,
                    SkipFurtherRemindersOnYesPressed = true,
                    AreFurtherRemindersSkipped = ApplicationUsageHelper.ApplicationRunsCountTotal > 10
                };
                rateAppReminder.Notify();
            }
        }

        string periodicTaskName = "CineworldPA";

        private void StartPeriodicAgent()
        {
            // Obtain a reference to the period task, if one exists
            ScheduledTask periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null)
            {
                try
                {
                    ScheduledActionService.Remove(periodicTaskName);
                }
                catch (Exception)
                {
                }
            }

            periodicTask = new PeriodicTask(periodicTaskName)
            {

                // The description is required for periodic agents. This is the string that the user
                // will see in the background services Settings page on the device.
                Description = "Download cineworld data and refresh tiles",
                ExpirationTime = DateTime.Today.AddDays(14)
            };

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);
                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG)
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
#endif
            }
            catch (Exception ex)
            {
                int i = 1;
            }
        }

        private async Task CheckNokiaMusicCountryCode()
        {
            if (Config.AllowNokiaMusicSearch == null)
            {
                string countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName.ToLower();
                CountryResolver resolver = new CountryResolver("1b41d6ef68154ed1a6f13814244c4134");
                Response<bool> resp = await resolver.CheckAvailabilityAsync(countryCode);

                Config.AllowNokiaMusicSearch = resp.Result;
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();
            RootFrame.UriMapper = new CustomUriMapper();

            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            //For Fast resume

            RootFrame.Navigating += RootFrame_Navigating;

            RootFrame.Navigated += RootFrame_Navigated;

        }

        /*The navigation args does not have a cancel method but you can check when it is being reset (i.e. when the app is starting or starting again) */

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {

            _reset = (e.NavigationMode == NavigationMode.Reset);

        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {

            /*not all navigation can be canceled e.IsCancelable is just helping not to through an exception.*/

            if (_reset && e.IsCancelable && e.Uri.OriginalString == "/Landing.xaml") /*the URI is the main page so this means it is starting or starting again*/
            {

                e.Cancel = true;

                _reset = false;

            }

        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}