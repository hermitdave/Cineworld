using Cineworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Cineworld_Android;

namespace Cineworld
{
    public class LocalStorageHelper : BaseStorageHelper
    {
        public async Task DeserialiseObjects()
        {
			if (CineworldApplication.CinemaFilms == null) 
			{
				CineworldApplication.CinemaFilms = new Dictionary<int, List<FilmInfo>> ();
				CineworldApplication.FilmCinemas = new Dictionary<int, List<CinemaInfo>> ();
			} else 
			{
				CineworldApplication.CinemaFilms.Clear ();
				CineworldApplication.FilmCinemas.Clear ();
			}
            Dictionary<int, List<CinemaInfo>> FilmCinemas = null;

			if (Config.Region == Config.RegionDef.UK)
			{
				CineworldApplication.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasUKFileName);
				CineworldApplication.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsUKFileName);

				FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasUKFileName);
			}
			else
			{
				CineworldApplication.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasIEFileName);
				CineworldApplication.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsIEFileName);

				FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasIEFileName);
			}

            foreach (var filmID in FilmCinemas.Keys)
            {
                List<CinemaInfo> cinemas = new List<CinemaInfo>();
                foreach (CinemaInfo ci in FilmCinemas[filmID])
					cinemas.Add(CineworldApplication.Cinemas[ci.ID]);

				CineworldApplication.FilmCinemas[filmID] = cinemas;
            }
        }

		public async Task<List<String>> GetPosterImages()
		{
			await DownloadFile(MovieFilmPostersFileName, false);

			var posters = await DeserialiseObject<List<String>> (MovieFilmPostersFileName);
			return posters;
		}

        public async Task<T> DeserialiseObject<T>(string file)
        {
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            try
            {
				using (Stream s = new FileStream(Path.Combine(documentsPath, file), FileMode.Open))
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
                return default(T);
            }
        }

		public override async Task<bool> GetCinemaFilmListings(int cinemaID, bool bForce = false)
        {
			string FileName = String.Format(FilmsPerCinemaFileName, cinemaID);
            
			bool downloaded = await base.GetCinemaFilmListings(cinemaID, bForce);

			if (!downloaded)
				return downloaded;

            List<FilmHeader> filmheaders = await DeserialiseObject<List<FilmHeader>>(FileName);
            List<FilmInfo> films = new List<FilmInfo>();
            foreach (var fh in filmheaders)
            {
                FilmInfo fi = null;

				if (CineworldApplication.Films.TryGetValue(fh.EDI, out fi))
                {
                    FilmInfo newfi = fi.Clone();

                    if (newfi != null)
                        newfi.Performances = fh.Performances;

                    foreach (var p in newfi.Performances)
                        p.FilmTitle = newfi.Title;

                    films.Add(newfi);
                }
            }
			CineworldApplication.CinemaFilms[cinemaID] = films;

			return true;
        }
    }
}