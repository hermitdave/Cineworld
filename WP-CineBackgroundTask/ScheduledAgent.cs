using System.Windows;
using Microsoft.Phone.Scheduler;
using System;
using Cineworld;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Data.Linq;
#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WP7Helpers.Common;
using Cineworld;
#endif

namespace WP_CineBackgroundTask
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override async void OnInvoke(ScheduledTask task)
        {
            Config.Initialise();

            int currentHour = DateTime.UtcNow.Hour;

            BaseStorageHelper bsh = new BaseStorageHelper();

            await bsh.DownloadPosters();

            List<Uri> images = await bsh.GetImageList();

            SetTileBackgroundImage(images);

            NotifyComplete();
        }

        private void SetTileBackgroundImage(List<Uri> images)
        {
            Random random = new Random();

            foreach(ShellTile currentTile in ShellTile.ActiveTiles)
            {
                SetTileBackground(images, random, currentTile);
            }
        }

        private static void SetTileBackground(List<Uri> images, Random random, ShellTile currentTile)
        {
            bool tryFlip = false;

            if (images.Count == 0)
                tryFlip = true;

            if (Config.IsTargetedVersion && !tryFlip)
            {
                try
                {
                    Uri smallimage = new Uri("Images/CycleSmall.png", UriKind.Relative);
                    Uri mediumimage = new Uri("Images/CycleMedium.png", UriKind.Relative);

                    UpdateCycleTile(smallimage, mediumimage, images, random, currentTile);
                }
                catch
                {
                    tryFlip = true;
                }
            }
            else
                tryFlip = true;
            
            if(tryFlip)
            {
                int rand = random.Next(0, images.Count - 1);

                StandardTileData NewTileData = new StandardTileData();
                if (Config.AnimateTiles && images.Count > 0)
                {
                    var schema = "isostore:/Shared/ShellContent/{0}";
                    NewTileData.BackBackgroundImage = new Uri(String.Format(schema, Path.GetFileName(images[rand].OriginalString)), UriKind.Absolute);
                }
                else
                    NewTileData.BackBackgroundImage = new Uri("Images/AppTile.png", UriKind.Relative);
                
                try
                {
                    currentTile.Update(NewTileData);
                }
                catch { }
            }
        }

        public static void UpdateCycleTile(
            Uri smallBackgroundImage, Uri backgroundImage,
            List<Uri> images, Random random, ShellTile currentTile)
        {
            // Get the new cycleTileData type.
            Type cycleTileDataType = Type.GetType("Microsoft.Phone.Shell.CycleTileData, Microsoft.Phone");

            // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
            Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

            // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
            var UpdateTileData = cycleTileDataType.GetConstructor(new Type[] { }).Invoke(null);

            // Set the properties. 
            SetProperty(UpdateTileData, "SmallBackgroundImage", smallBackgroundImage);

            Uri[] mediumImages = new Uri[9];

            mediumImages[0] = backgroundImage;

            if (Config.AnimateTiles)
            {
                var schema = "isostore:/Shared/ShellContent/{0}";
                
                for (int i = 1; i < 9; i++)
                {
                    int rand = random.Next(images.Count - 1);

                    mediumImages[i] = new Uri(String.Format(schema, Path.GetFileName(images[rand].OriginalString)), UriKind.Absolute);
                }
            }

            SetProperty(UpdateTileData, "CycleImages", mediumImages);

            // Invoke the new version of ShellTile.Update.
            shellTileType.GetMethod("Update").Invoke(currentTile, new Object[] { UpdateTileData });
        }

        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }
    }
}