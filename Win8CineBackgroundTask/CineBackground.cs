using Cineworld;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace Win8CineBackgroundTask
{
    public sealed class CineBackground : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            int currentHour = DateTime.UtcNow.Hour;

            if (currentHour > 4 && currentHour < 6)
            {
                List<Task> tasks = new List<Task>();

                tasks.Add(new BaseStorageHelper().DownloadFiles());

                HashSet<int> preferrredCinemas = new HashSet<int>();

                char[] cSep = { '&', '=' };

                foreach (var tile in await SecondaryTile.FindAllAsync())
                {
                    int iCin = int.Parse(tile.TileId); 
                    if(!preferrredCinemas.Contains(iCin))
                        preferrredCinemas.Add(iCin);
                }

                foreach (int cin in preferrredCinemas)
                {
                    tasks.Add(new BaseStorageHelper().GetCinemaFilmListings(cin));
                }

                await Task.WhenAll(tasks);
            }

            await SetTileBackgroundImage();
            
            deferral.Complete();
        }

        private async Task SetTileBackgroundImage()
        {

            StorageFile inputfile = await ApplicationData.Current.LocalFolder.GetFileAsync(BaseStorageHelper.FilmPostersFileName);

            object o = null;
            try
            {
                using (Stream s = await inputfile.OpenStreamForReadAsync())
                {
                    using (var gzipStream = new GZipStream(s, CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new StreamReader(gzipStream))
                        {
                            o = JsonConvert.DeserializeObject(sr.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            if (o == null || !(o is JArray))
                return;

            JArray data = (JArray)o;

            Random random = new Random();

            string format = "<tile><visual><binding template=\"TileSquareImage\"><image id=\"1\" src=\"{0}\"/></binding></visual></tile>";

            int rand = random.Next(0, data.Count-1);

            TileUpdater mainUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            XmlDocument mainDoc = new XmlDocument();
            mainDoc.LoadXml(String.Format(format, (string)data[rand]));
            mainUpdater.Update(new TileNotification(mainDoc));

            foreach (var tile in await SecondaryTile.FindAllAsync())
            {
                rand = random.Next(0, data.Count);

                tile.Logo = new Uri((string)data[rand]);

                await tile.UpdateAsync();
            }
        }
    }
}
