using Cineworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using CineworldiPhone;

namespace Cineworld
{
    public class LocalStorageHelper : BaseStorageHelper
    {
        public async Task DeserialiseObjects()
        {
			if (Application.CinemaFilms == null) 
			{
				Application.CinemaFilms = new Dictionary<int, List<FilmInfo>> ();
				Application.FilmCinemas = new Dictionary<int, List<CinemaInfo>> ();
			} else 
			{
				Application.CinemaFilms.Clear ();
				Application.FilmCinemas.Clear ();
			}
            Dictionary<int, List<CinemaInfo>> FilmCinemas = null;

			if (Config.Region == Config.RegionDef.UK)
			{
				Application.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasUKFileName);
				Application.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsUKFileName);

				FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasUKFileName);
			}
			else
			{
				Application.Cinemas = await DeserialiseObject<Dictionary<int, CinemaInfo>>(CinemasIEFileName);
				Application.Films = await DeserialiseObject<Dictionary<int, FilmInfo>>(FilmsIEFileName);

				FilmCinemas = await DeserialiseObject<Dictionary<int, List<CinemaInfo>>>(FilmCinemasIEFileName);
			}

            foreach (var filmID in FilmCinemas.Keys)
            {
                List<CinemaInfo> cinemas = new List<CinemaInfo>();
                foreach (CinemaInfo ci in FilmCinemas[filmID])
					cinemas.Add(Application.Cinemas[ci.ID]);

				Application.FilmCinemas[filmID] = cinemas;
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

        public override async Task GetCinemaFilmListings(int cinemaID, bool bForce = false)
        {
            string FileName = String.Format(FilmsPerCinemaFileName, cinemaID);
            
            await base.GetCinemaFilmListings(cinemaID, bForce);

            List<FilmHeader> filmheaders = await DeserialiseObject<List<FilmHeader>>(FileName);
            List<FilmInfo> films = new List<FilmInfo>();
            foreach (var fh in filmheaders)
            {
                FilmInfo fi = null;

				if (Application.Films.TryGetValue(fh.EDI, out fi))
                {
                    FilmInfo newfi = fi.Clone();

                    if (newfi != null)
                        newfi.Performances = fh.Performances;

                    foreach (var p in newfi.Performances)
                        p.FilmTitle = newfi.Title;

                    films.Add(newfi);
                }
            }
			Application.CinemaFilms[cinemaID] = films;
        }
    }
}