using Cineworld.Common;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace Cineworld
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Configuration TMDBConfig { get; set; }

        public static bool IsFree { get; set; }

        public static ListingInformation ListingInfo { get; set; }

        public const string AdFreeIAP = "AdFree";

        public static List<string> Posters { get; set; }

        public static Dictionary<int, FilmInfo> Films { get; set; }

        public static Dictionary<int, CinemaInfo> Cinemas { get; set; }

        public static Dictionary<int, List<FilmInfo>> CinemaFilms { get; set; }

        public static Dictionary<int, List<CinemaInfo>> FilmCinemas { get; set; }

        //public static MobileServiceClient MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");
        public static MobileServiceClient MobileService = null;

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            Posters = new List<string>();
            Films = new Dictionary<int, FilmInfo>();
            Cinemas = new Dictionary<int, CinemaInfo>();
            CinemaFilms = new Dictionary<int, List<FilmInfo>>();
            FilmCinemas = new Dictionary<int, List<CinemaInfo>>();

            Config.Initialise();

            App.Current.RequestedTheme = Config.Theme;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await EnsureAppActivated(args);
        }



        private static async Task EnsureAppActivated(IActivatedEventArgs args)
        {
            if(CurrentApp.LicenseInformation != null && CurrentApp.LicenseInformation.ProductLicenses != null && CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(AdFreeIAP))
            {
                App.IsFree = !CurrentApp.LicenseInformation.ProductLicenses[AdFreeIAP].IsActive;
            }
            else
            {
                App.IsFree = true;
            }

            if(App.IsFree)
            {
                try
                {
                    App.ListingInfo = await CurrentApp.LoadListingInformationAsync();
                }
                catch { }
            }

            int iCin = int.MinValue;

            if (args is LaunchActivatedEventArgs)
            {
                LaunchActivatedEventArgs largs = args as LaunchActivatedEventArgs;
                if (!String.IsNullOrEmpty(largs.Arguments) && !String.IsNullOrEmpty(largs.TileId))
                    iCin = int.Parse(largs.TileId);

                if (!String.IsNullOrEmpty(largs.Arguments))
                {
                    Config.RegionDef r = (Config.RegionDef)Enum.Parse(typeof(Config.RegionDef), largs.Arguments);
                    if (Config.Region != r)
                        Config.Region = r;
                }
            }

            Frame rootFrame = null;

            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                if (iCin == int.MinValue)
                {
                    Window.Current.Activate();
                    return;
                }
                else
                    rootFrame = (Frame)Window.Current.Content;
            }
            else
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                rootFrame = new Frame();
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
            }

            //if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            //{
            //    // Restore the saved session state only when appropriate
            //    await SuspensionManager.RestoreAsync();
            //}

            if (rootFrame.Content == null || iCin != int.MinValue)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                if (iCin != int.MinValue)
                {
                    Config.ShowRegion = false;
                    //CinemaDetails.CinemaID = iCin;
                    rootFrame.Navigate(typeof(CinemaDetails), iCin);
                }
                else
                {
                    Config.ShowRegion = true;

                    if (!rootFrame.Navigate(typeof(Landing)))
                    {
                        throw new Exception("Failed to create initial page");
                    }
                }
            }

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

            int started = 0;
            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("started"))
            {
                started = (int)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["started"];
            }

            started++;

            Windows.Storage.ApplicationData.Current.RoamingSettings.Values["started"] = started;

            string familyName = Package.Current.Id.FamilyName;

            if ((started == 5 || started == 10) && !Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("rated"))
            {
                var md = new Windows.UI.Popups.MessageDialog("We'd love you to rate our app 5 stars\nShowing us some love on the store helps us to continue to work on the app and make things even better!", "Enjoying Cineworld app?");
                bool? reviewresult = null;
                md.Commands.Add(new Windows.UI.Popups.UICommand("OK", new Windows.UI.Popups.UICommandInvokedHandler((cmd) => reviewresult = true)));
                md.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", new Windows.UI.Popups.UICommandInvokedHandler((cmd) => reviewresult = false)));
                await md.ShowAsync();
                if (reviewresult == true)
                {
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values["rated"] = true;

                    await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", familyName)));
                }
            } 


            ////Only when the debugger is attached
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    //Display the metro grid helper
            //    MC.MetroGridHelper.MetroGridHelper.CreateGrid();
            //}
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted

            // event in OnWindowCreated to speed up searches once the application is already running

            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // If the app does not contain a top-level frame, it is possible that this 
            // is the initial launch of the app. Typically this method and OnLaunched 
            // in App.xaml.cs can call a common method.
            if (frame == null)
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                frame = new Frame();
                Cineworld.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await Cineworld.Common.SuspensionManager.RestoreAsync();
                    }
                    catch //(Cineworld.Common.SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }
            }

            frame.Navigate(typeof(SearchResults), args.QueryText);
            Window.Current.Content = frame;

            // Ensure the current window is active
            Window.Current.Activate();
        }
    }
}
