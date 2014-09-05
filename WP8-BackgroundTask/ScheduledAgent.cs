using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Collections.Generic;
using System;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Cineworld;
using Windows.Phone.System.UserProfile;
using Cineworld;
using System.IO.IsolatedStorage;
using System.IO;

namespace WP8_BackgroundTask
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
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

            BaseStorageHelper bsh = new BaseStorageHelper();

            int currentHour = DateTime.UtcNow.Hour;

            if (currentHour > 4 && currentHour < 23)
            {
                List<Task> tasks = new List<Task>();

                tasks.Add(bsh.DownloadFiles());

                HashSet<int> preferrredCinemas = new HashSet<int>();

                if (Config.FavCinemas != null)
                {
                    foreach (var cinema in Config.FavCinemas)
                        preferrredCinemas.Add(cinema);
                }

                char[] cSep = { '&', '=' };

                foreach (var tile in ShellTile.ActiveTiles)
                {
                    string[] parts = tile.NavigationUri.ToString().Split(cSep, StringSplitOptions.RemoveEmptyEntries);
                    int iCin = -1;

                    if (parts.Length == 2)
                    {
                        // cinema id is second part
                        iCin = int.Parse(parts[1]);
                    }
                    else if (parts.Length == 4)
                    {
                        // cinema id is second part
                        iCin = int.Parse(parts[1]);

                        if (Config.Region != (Config.RegionDef)int.Parse(parts[3]))
                            continue;
                    }
                    else
                        continue;

                    if (iCin != -1 && !preferrredCinemas.Contains(iCin))
                        preferrredCinemas.Add(iCin);
                }

                foreach (int cin in preferrredCinemas)
                {
                    tasks.Add(new BaseStorageHelper().GetCinemaFilmListings(cin));
                }

                await Task.WhenAll(tasks);
            }

            await bsh.DownloadPosters();

            List<Uri> images = await bsh.GetImageList(false);

            SetTileBackgroundImage(images);

            await LockScreenChange(images);

            NotifyComplete();
        }

        public static async Task LockScreenChange(List<Uri> images)
        {
                
            if (images.Count == 0)
                return;

            Random random = new Random();

            Uri randomUri = images[random.Next(images.Count - 1)];

            await SetPoster(randomUri);
        }

        public static async Task SetPoster(Uri randomUri)
        {
            // Only do further work if the access is granted.
            if (!LockScreenManager.IsProvidedByCurrentApplication || !Config.AnimateLockscreen)
                return;
            
            string poster = Path.GetFileName(randomUri.AbsoluteUri);

            Config.CurrentLockscreen = poster;

            // At this stage, the app is the active lock screen background provider.
            // The following code example shows the new URI schema.
            // ms-appdata points to the root of the local app data folder.
            // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package
            string folder = "Shared\\ShellContent";
            var schema = "ms-appdata:///Local/Shared/ShellContent/{0}";
            var uri = new Uri(String.Format(schema, poster), UriKind.Absolute);

            string localPath = String.Format("{0}\\{1}", folder, poster);
            if (!IsolatedStorageFile.GetUserStoreForApplication().FileExists(localPath))
            {
                await new AsyncWebClient().DownloadFileAsync(randomUri.OriginalString, poster, folder);
            }

            // Set the lock screen background image.
            LockScreen.SetImageUri(uri);
        }

        private void SetTileBackgroundImage(List<Uri> images)
        {
            //BaseStorageHelper helper = new BaseStorageHelper();
            //List<Uri> images = await helper.GetTileImageList();

            Random random = new Random();

            foreach (ShellTile currentTile in ShellTile.ActiveTiles)
            {
                SetTileBackground(images, random, currentTile);
            }
        }

        private static void SetTileBackground(List<Uri> images, Random random, ShellTile currentTile)
        {
            bool tryFlip = false;

            if (images.Count == 0)
                tryFlip = true;

            if (!tryFlip)
            {
                try
                {
                    Uri smallimage = new Uri("Images/FlipCycleSmall.png", UriKind.Relative);
                    Uri mediumimage = new Uri("Images/FlipCycleMedium.png", UriKind.Relative);

                    UpdateCycleTile(smallimage, mediumimage, images, random, currentTile);
                }
                catch
                {
                    tryFlip = true;
                }
            }
        
            if (tryFlip)
            {
                FlipTileData NewTileData = new FlipTileData();
                if (Config.AnimateTiles && images.Count > 0)
                {
                    int rand = random.Next(0, images.Count - 1);

                    var schema = "isostore:/Shared/ShellContent/{0}";
                    NewTileData.BackBackgroundImage = new Uri(String.Format(schema, Path.GetFileName(images[rand].OriginalString)), UriKind.Absolute);
                }
                else
                {   
                    NewTileData.BackBackgroundImage = new Uri("Images/FlipCycleMedium.png", UriKind.Relative);
                }

                //NewTileData.SmallBackgroundImage = new Uri("Images/FlipCycleSmall.png", UriKind.Relative);
                //NewTileData.BackgroundImage = new Uri("Images/FlipCycleMedium.png", UriKind.Relative);

                
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
            Uri[] mediumImages = new Uri[9];

            mediumImages[0] = backgroundImage;
            
            if (Config.AnimateTiles)
            {
                var schema = "isostore:/Shared/ShellContent/{0}";
                    
                for (int i = 1; i < 9; i++)
                {
                    int rand = random.Next(images.Count - 1);
                    
                    mediumImages[i] =  new Uri(String.Format(schema, Path.GetFileName(images[rand].OriginalString)), UriKind.Absolute);
                }
            }

            // Get the new cycleTileData type.
            CycleTileData cycleTileData = new CycleTileData
            {
                // Set the properties. 
                SmallBackgroundImage = smallBackgroundImage,
                CycleImages = mediumImages
            };

            // Invoke the new version of ShellTile.Update.
            currentTile.Update(cycleTileData);
        }
    }
}