using Cineworld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace Cineworld
{
    public class BaseStorageHelper
    {
        public string blobStorage = "http://Cineworlddata.blob.core.windows.net/data/";
        public const string CinemasUKFileName = "cinemas.uk.gz";
        public const string FilmsUKFileName = "films.uk.gz";
        public const string FilmCinemasUKFileName = "filmcinemas.uk.gz";

        public const string CinemasIEFileName = "cinemas.ie.gz";
        public const string FilmsIEFileName = "films.ie.gz";
        public const string FilmCinemasIEFileName = "filmcinemas.ie.gz";
        public const string FilmsPerCinemaFileName = "{0}.films.gz";

        public const string MovieFilmPostersFileName = "posters.txt.gz";

        public async Task DownloadFiles(bool bForce = false)
        {
            List<Task> tasks = new List<Task>();
			tasks.Add(DownloadFile(Config.Region == Config.RegionDef.UK ? CinemasUKFileName : CinemasIEFileName, bForce));
			tasks.Add(DownloadFile(Config.Region == Config.RegionDef.UK ? FilmsUKFileName : FilmsIEFileName, bForce));
			tasks.Add(DownloadFile(Config.Region == Config.RegionDef.UK ? FilmCinemasUKFileName : FilmCinemasIEFileName, bForce));
            
//			tasks.Add(DownloadFile(CinemasUKFileName, bForce));
//			tasks.Add(DownloadFile(FilmsUKFileName, bForce));
//			tasks.Add(DownloadFile(FilmCinemasUKFileName, bForce));


            await Task.WhenAll(tasks);
        }

#if WINDOWS_PHONE
        public async Task DownloadPosters()
        {
            await DownloadFile(MovieFilmPostersFileName, false);

            if (!Config.AnimateLockscreen)
                return;

            object o = await ReadMoviePosters();

            if (o == null || !(o is JObject))
                return;

            JObject data = (JObject)o;

            bool downloadFiles = true;

            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            string folder = "Shared\\ShellContent";
            
            if (!isf.DirectoryExists(folder))
                isf.CreateDirectory(folder);
            else
            {
                if (isf.GetLastWriteTime(folder) >= isf.GetLastWriteTime(BaseStorageHelper.MovieFilmPostersFileName) && isf.GetFileNames("*.jpg").Length > 0)
                    downloadFiles = false;
            }

            if (downloadFiles)
            {
                AsyncWebClient awc = new AsyncWebClient();

                HashSet<string> newPosters = new HashSet<string>();

                foreach (var item in data)
                {
                    string filename = Path.GetFileName((string)item.Value);
                    newPosters.Add(filename);
                    string localPath = String.Format("{0}\\{1}", folder, filename);
                    if (!isf.FileExists(localPath))
                    {
                        await awc.DownloadFileAsync((string)item.Value, filename, folder);
                    }

                }
                
                HashSet<string> allPosters = new HashSet<string>(isf.GetFileNames(String.Format("{0}\\*.*", folder)));

                allPosters.ExceptWith(newPosters);

                foreach (var file in allPosters)
                {
                    try
                    {
                        isf.DeleteFile(file);
                    }
                    catch { }
                }

                allPosters.ExceptWith(newPosters);

                foreach (var file in allPosters)
                {
                    try
                    {
                        isf.DeleteFile(file);
                    }
                    catch { }
                }
            }
        }
#endif
        
        public async Task<List<Uri>> GetImageList(bool bGetAllImages = true)
        {
            List<Uri> images = new List<Uri>();

            object o = await ReadMoviePosters();

            if (o == null || !(o is JObject))
                return images;

            JObject data = (JObject)o;

#if WINDOWS_PHONE
            HashSet<int> ignoredFilms = new HashSet<int>(Config.FilmPostersToIgnore);
#else
            HashSet<int> ignoredFilms = new HashSet<int>();
#endif
            HashSet<string> igoredPosters = new HashSet<string>();

            HashSet<string> uniqueposters = new HashSet<string>();

            foreach (var item in data)
            {
                bool bProcess = true;

                string poster = (string)item.Value;

                if (!bGetAllImages)
                {
                    int film = int.Parse(item.Key);

                    if (ignoredFilms.Contains(film))
                    {
                        if (!igoredPosters.Contains(poster))
                            igoredPosters.Add(poster);

                        bProcess = false;
                    }

                    if (igoredPosters.Contains(poster) && !ignoredFilms.Contains(film))
                    {
                        ignoredFilms.Add(film);
                        bProcess = false;
                    }
                }

                if (bProcess && !uniqueposters.Contains(poster))
                {
                    uniqueposters.Add(poster);
                    images.Add(new Uri(poster, UriKind.Absolute));
                }
            }

            return images;
        }

        public async Task<object> ReadMoviePosters()
        {
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            await DownloadFile(MovieFilmPostersFileName, false);

            object o = null;
            try
            {
				using (Stream s = new FileStream(Path.Combine(documentsPath, BaseStorageHelper.MovieFilmPostersFileName), FileMode.Open))
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
            return o;
        }

        public void CleanSharedContent()
        {
#if WINDOWS_PHONE
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

            string folder = "Shared\\ShellContent";

            string[] files = isf.GetFileNames(String.Format("{0}\\*.*", folder));

            foreach (string file in files)
            {
                string fileToDelete = String.Format("{0}\\{1}", folder, file);
                try
                {
                    isf.DeleteFile(fileToDelete);
                }
                catch { }
            }
#endif
        }

        public async Task DownloadFile(string file, bool bForce, bool bCinemaPerformaceData = false)
        {
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

			if (bForce || !File.Exists(file) || DateTime.Now.Subtract(File.GetLastWriteTime(file)).TotalHours > 24)
				await (new AsyncWebClient()).DownloadFileAsync(String.Format("{0}{1}", blobStorage, file), file, documentsPath);
        }


        public virtual async Task GetCinemaFilmListings(int cinemaID, bool bForce = false)
        {
            string FileName = String.Format(FilmsPerCinemaFileName, cinemaID);
            
            bool bCinemaListing = true;
            
            await DownloadFile(FileName, bForce, bCinemaListing);
       }
    }
}
