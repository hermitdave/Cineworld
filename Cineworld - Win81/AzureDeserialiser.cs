using CineWorld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Win8CineBackgroundTask;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Cineworld
{
    public class LocalStorageHelper
    {
        string blobStorage = "http://cineworlddata.blob.core.windows.net/data/";
        const string CinemasUKFileName = "cinemas.uk.gz";
        const string FilmsUKFileName = "films.uk.gz";
        const string CinemaFilmsUKFileName = "cinemafilms.uk.gz";
        const string FilmCinemasUKFileName = "filmcinemas.uk.gz";

        const string CinemasIEFileName = "cinemas.ie.gz";
        const string FilmsIEFileName = "films.ie.gz";
        const string CinemaFilmsIEFileName = "cinemafilms.ie.gz";
        const string FilmCinemasIEFileName = "filmcinemas.ie.gz";

        public async Task DownloadFiles(bool bForce = false)
        {
            AsyncWebClient awc = new AsyncWebClient();

            if (Config.Region == Config.RegionDef.UK)
            {
                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, CinemasUKFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, CinemasUKFileName), CinemasUKFileName);

                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, FilmsUKFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, FilmsUKFileName), FilmsUKFileName);

                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, CinemaFilmsUKFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, CinemaFilmsUKFileName), CinemaFilmsUKFileName);

                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, FilmCinemasUKFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, FilmCinemasUKFileName), FilmCinemasUKFileName);
            }
            else
            {
                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, CinemasIEFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, CinemasIEFileName), CinemasIEFileName);

                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, FilmsIEFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, FilmsIEFileName), FilmsIEFileName);

                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, CinemaFilmsIEFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, CinemaFilmsIEFileName), CinemaFilmsIEFileName);

                if (!await PersistHelper.FileExisits(ApplicationData.Current.LocalFolder, FilmCinemasIEFileName) || bForce)
                    await awc.DownloadFileAsync(String.Format("{0}{1}", blobStorage, FilmCinemasIEFileName), FilmCinemasIEFileName);
            }
        }

        public async Task DeserialiseObjects()
        {
            App.CinemaFilms.Clear();
            App.FilmCinemas.Clear();

            Dictionary<int, List<FilmInfo>> CinemaFilms = null;
            Dictionary<int, List<CinemaInfo>> FilmCinemas = null;

            if (Config.Region == Config.RegionDef.UK)
            {
                App.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasUKFileName);
                App.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsUKFileName);

                CinemaFilms = await DeserialiseObject<Dictionary<int, List<FilmInfo>>>(CinemaFilmsUKFileName);
                FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasUKFileName);
            }
            else
            {
                App.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasIEFileName);
                App.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsIEFileName);

                CinemaFilms = await DeserialiseObject<Dictionary<int, List<FilmInfo>>>(CinemaFilmsIEFileName);
                FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasIEFileName);
            }

            foreach (var film in App.Films.Values)
            {
                if (film.PosterUrl == null)
                {
                    film.PosterUrl = new Uri("ms-appx:/Assets/PlaceHolder.png");
                }
            }

            foreach (var cinID in CinemaFilms.Keys)
            {
                List<FilmInfo> films = new List<FilmInfo>();
                foreach (FilmInfo fi in CinemaFilms[cinID])
                    films.Add(App.Films[fi.EDI]);
                
                App.CinemaFilms[cinID] = films;
            }

            foreach (var filmID in FilmCinemas.Keys)
            {
                List<CinemaInfo> cinemas = new List<CinemaInfo>();
                foreach (CinemaInfo ci in FilmCinemas[filmID])
                    cinemas.Add(App.Cinemas[ci.ID]);

                App.FilmCinemas[filmID] = cinemas;
            }
        }

        public async Task<T> DeserialiseObject<T>(string file)
        {
            try
            {
                StorageFile inputfile = await ApplicationData.Current.LocalFolder.GetFileAsync(file);

                using (Stream s = await inputfile.OpenStreamForReadAsync())
                {
                    using (var gzipStream = new GZipStream(s, CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new StreamReader(gzipStream))
                        {
                            return (T)JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (1 == 1)
                {
                }

                return default(T);
            }
        }
    }

    //public class AzureHelper
    //{
    //    private const string TASK_NAME = "CineBackground";
    //    private const string TASK_ENTRY = "BackgroundTasks.CineBackground";

    //    const string CinemasUKFileName = "cinemas.uk.gz";
    //    const string FilmsUKFileName = "films.uk.gz";
    //    const string CinemaFilmsUKFileName = "cinemafilms.uk.gz";
    //    const string FilmCinemasUKFileName = "filmcinemas.uk.gz";

    //    const string CinemasIEFileName = "cinemas.ie.gz";
    //    const string FilmsIEFileName = "films.ie.gz";
    //    const string CinemaFilmsIEFileName = "cinemafilms.ie.gz";
    //    const string FilmCinemasIEFileName = "filmcinemas.ie.gz";

        
    //    public async Task DeserialiseObjects()
    //    {
    //        await EnsureBackgroundTask();

    //        App.Cinemas = await DeserialiseObject<List<CinemaInfo>>(CinemasUKFileName);
    //        App.Films = await DeserialiseObject<List<FilmInfo>>(FilmsUKFileName);
    //        App.CinemaFilms = await DeserialiseObject<Dictionary<int , List<FilmInfo>>>(CinemaFilmsUKFileName);
    //        App.FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasUKFileName);
    //    }

    //    public async Task<T> DeserialiseObject<T>(string file)
    //    {
    //        StorageFile inputfile = await ApplicationData.Current.LocalFolder.GetFileAsync(file);

    //        using (Stream s = await inputfile.OpenStreamForReadAsync())
    //        {
    //            using (var gzipStream = new GZipStream(s, CompressionMode.Decompress))
    //            {
    //                using (StreamReader sr = new StreamReader(gzipStream))
    //                {
    //                    return (T)JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
    //                }
    //            }
    //        }
    //    }

    //    public async Task EnsureBackgroundTask()
    //    {
    //        BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
    //        builder.Name = TASK_NAME;
    //        builder.TaskEntryPoint = TASK_ENTRY;
    //        builder.SetTrigger(new TimeTrigger(15, false));
    //        var registration = builder.Register();
    //    }
    //}
}
