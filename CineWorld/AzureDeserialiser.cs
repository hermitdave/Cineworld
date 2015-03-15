using Cineworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Win8CineBackgroundTask;
#if WINDOWS_PHONE
//using Ionic.Zlib;
using System.IO.IsolatedStorage;
using CineWorld;
using System.IO.Compression;
#else
using System.IO.Compression;
using Windows.ApplicationModel.Background;
using Windows.Storage;
#endif

namespace Cineworld
{
    public class LocalStorageHelper : BaseStorageHelper
    {
        public async Task DeserialiseObjects()
        {
            App.CinemaFilms.Clear();
            App.FilmCinemas.Clear();

            Dictionary<int, List<CinemaInfo>> FilmCinemas = null;

            if (Config.Region == Config.RegionDef.UK)
            {
                App.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasUKFileName);
                App.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsUKFileName);

                FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasUKFileName);
            }
            else
            {
                App.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasIEFileName);
                App.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsIEFileName);

                FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasIEFileName);
            }

            foreach (var film in App.Films.Values)
            {
                if (film.PosterUrl == null)
                {
#if WINDOWS_PHONE
                    film.PosterUrl = new Uri("Images/PlaceHolder.png", UriKind.Relative);
#else
                    film.PosterUrl = new Uri("ms-appx:/Assets/PlaceHolder.png");
#endif
                }
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
#if WINDOWS_PHONE
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                using (Stream s = isf.OpenFile(file, FileMode.Open))
                {
#else
                StorageFile inputfile = await ApplicationData.Current.LocalFolder.GetFileAsync(file);

                using (Stream s = await inputfile.OpenStreamForReadAsync())
                {
#endif
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
                return default(T);
            }
        }

        public override async Task GetCinemaFilmListings(int cinemaID, bool bForce = false)
        {
            string FileName = String.Format(FilmsPerCinemaFileName, cinemaID);
            
            await base.GetCinemaFilmListings(cinemaID, bForce);

            List<FilmHeader> filmheaders = await DeserialiseObject<List<FilmHeader>>(FileName);
            List<FilmInfo> films = new List<FilmInfo>();
            foreach (var fh in filmheaders)
            {
                FilmInfo fi = null;

                if (App.Films.TryGetValue(fh.EDI, out fi))
                {
                    FilmInfo newfi = fi.Clone();

                    if (newfi != null)
                        newfi.Performances = fh.Performances;

                    foreach (var p in newfi.Performances)
                        p.FilmTitle = newfi.Title;

                    films.Add(newfi);
                }
            }
            App.CinemaFilms[cinemaID] = films;
        }
    }
}