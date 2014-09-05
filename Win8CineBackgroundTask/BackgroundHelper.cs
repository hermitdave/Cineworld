using Cineworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace Win8CineBackgroundTask
{
    public class BackgroundHelper
    {
        static HashSet<int> PinnedCinemas = new HashSet<int>();
            
        public static void Execute()
        {
            Task t = DownloadFiles();
            t.Wait();

            CreateSchedule();

            foreach (int cin in PinnedCinemas)
                CreateSchedule(cin);
        }

        public static async Task DownloadFiles()
        {
            List<Task> tasks = new List<Task>();
            LocalStorageHelper lsh = new LocalStorageHelper();
            tasks.Add(lsh.DownloadFiles());

            Geolocator locator = new Geolocator();
            Task<Geoposition> tpos = locator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10)).AsTask();
            
            foreach (var tile in await SecondaryTile.FindAllAsync())
            {
                int iCin = int.Parse(tile.TileId);
                PinnedCinemas.Add(iCin);
                tasks.Add(lsh.GetCinemaFilmListings(iCin));
            }

            foreach (int iCin in Config.FavCinemas)
            {
                if (PinnedCinemas.Contains(iCin))
                    continue;

                PinnedCinemas.Add(iCin);
                tasks.Add(lsh.GetCinemaFilmListings(iCin));
            }

            Geoposition pos = await tpos;

            IEnumerable<CinemaInfo> oc = App.Cinemas.Values.OrderBy(c => GeoMath.Distance(pos.Coordinate.Latitude, pos.Coordinate.Longitude, c.Latitude, c.Longitute, GeoMath.MeasureUnits.Miles)).Take(1);
            if (oc.Count() > 0)
            {
                int iCin = oc.First().ID;

                if (!PinnedCinemas.Contains(iCin))
                    tasks.Add(lsh.GetCinemaFilmListings(iCin));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public static void CreateSchedule(int cinemaid = -1)
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();

            foreach (var item in updater.GetScheduledTileNotifications())
            {
                if (DateTime.Now > item.ExpirationTime)
                    updater.RemoveFromSchedule(item);
            }

            IReadOnlyList<ScheduledTileNotification> scheduledTileNotifications = updater.GetScheduledTileNotifications();
            DateTime time = DateTime.Now;
            DateTime time2 = time.AddHours(2.0);
            DateTime time3 = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0).AddMinutes(1.0);
            if (scheduledTileNotifications.Count > 0)
            {
                DateTime dtTemp = scheduledTileNotifications[scheduledTileNotifications.Count - 1].DeliveryTime.DateTime.AddMinutes(1);
                if (dtTemp > time3)
                    time3 = dtTemp;
            }
            string format = "<tile><visual><binding template=\"TileSquareImage\"><image id=\"1\" src=\"{0}\"/></binding></visual></tile>";
            
            for (DateTime time4 = time3; time4 < time2; time4 = time4.AddMinutes(1.0))
            {
                try
                {
                    List<Uri> uris = App.GetRandomImages(cinemaid);

                    string str2 = string.Format(format, uris[0], uris[1], uris[2], uris[3], uris[4]);
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(str2);
                    ScheduledTileNotification notification2 = new ScheduledTileNotification(document, new DateTimeOffset(time4));
                    notification2.ExpirationTime = (new DateTimeOffset?((DateTimeOffset)time4.AddMinutes(1.0)));
                    ScheduledTileNotification notification = notification2;
                    updater.AddToSchedule(notification);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
