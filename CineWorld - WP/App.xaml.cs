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
using System.Xml;
using Microsoft.Phone.Tasks;

namespace CineWorld
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static Configuration TMDBConfig { get; set; }

        public static List<string> Posters { get; set; }

        public static Dictionary<int, FilmInfo> Films { get; set; }

        public static Dictionary<int, CinemaInfo> Cinemas { get; set; }

        public static Dictionary<int, List<FilmInfo>> CinemaFilms { get; set; }

        public static Dictionary<int, List<CinemaInfo>> FilmCinemas { get; set; }

        //public static MobileServiceClient MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/","kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");
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

            Posters = new List<string>();
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
                Application.Current.Host.Settings.EnableFrameRateCounter = false;

                //MetroGridHelper.IsVisible = true;

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
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //CheckForUpdatedVersion();

            try
            {
                this.StartPeriodicAgent();
            }
            finally
            {
                // Call this on launch to initialise the feedback helper
                //NokiaFeedbackDemo.Helpers.FeedbackHelper.Default.Launching();
            }
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
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
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

        //public static string GetManifestAttributeValue(string attributeName)
        //{
        //    var xmlReaderSettings = new XmlReaderSettings
        //    {
        //        XmlResolver = new XmlXapResolver()
        //    };

        //    using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
        //    {
        //        xmlReader.ReadToDescendant("App");

        //        return xmlReader.GetAttribute(attributeName);
        //    }
        //}

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
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
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