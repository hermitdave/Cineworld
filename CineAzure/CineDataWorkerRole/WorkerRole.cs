using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using Cineworld;
using System.Globalization;
using Newtonsoft.Json;
using System.Configuration;
using System.IO.Compression;
using System.IO;
using System.ServiceModel;
using CineDataWorkerRole.MobileService;
using Microsoft.Azure;

namespace CineDataWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        ServiceHost serviceHost = new ServiceHost(typeof(CineMobileService));

        string CinemasUKFileName = "cinemas.uk.gz";
        string FilmsUKFileName = "films.uk.gz";
        string CinemaFilmsUKFileName = "cinemafilms.uk.gz";
        string FilmCinemasUKFileName = "filmcinemas.uk.gz";

        string CinemasIEFileName = "cinemas.ie.gz";
        string FilmsIEFileName = "films.ie.gz";
        string CinemaFilmsIEFileName = "cinemafilms.ie.gz";
        string FilmCinemasIEFileName = "filmcinemas.ie.gz";
        string FilmsPerCinemaFileName = "{0}.films.gz";
        
        string MovieFilmPostersFileName = "movieposters.txt.gz";
        
        string LogFile = "{0}.log";

        //Dictionary<int, Movie> tmdbMovieDictionary = new Dictionary<int, Movie>();
        //Dictionary<int, int> filmMovieDictionary = new Dictionary<int, int>();

        Dictionary<int, double> LongitudeDictionary = new Dictionary<int, double>();
        Dictionary<int, double> LatitudeDictionary = new Dictionary<int, double>();

        Dictionary<int, List<FilmHeader>> dictionaryCinemaFilms = new Dictionary<int, List<FilmHeader>>();
        Dictionary<int, CinemaInfo> CinemasUK = new Dictionary<int, CinemaInfo>();
        Dictionary<int, FilmInfo> FilmsUK = new Dictionary<int, FilmInfo>();
        Dictionary<int, List<FilmInfo>> cinemaFilmsUK = new Dictionary<int, List<FilmInfo>>();
        Dictionary<int, List<CinemaInfo>> filmCinemasUK = new Dictionary<int, List<CinemaInfo>>();

        Dictionary<int, CinemaInfo> CinemasIE = new Dictionary<int, CinemaInfo>();
        Dictionary<int, FilmInfo> FilmsIE = new Dictionary<int, FilmInfo>();
        Dictionary<int, List<FilmInfo>> cinemaFilmsIE = new Dictionary<int, List<FilmInfo>>();
        Dictionary<int, List<CinemaInfo>> filmCinemasIE = new Dictionary<int, List<CinemaInfo>>();
        //Dictionary<int, List<int>> todayCinemaFilms = new Dictionary<int, List<int>>();

        Dictionary<int, string> MoviePosters = new Dictionary<int, string>();
        
        DateTime dtNextFullRun = DateTime.MinValue;
        

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("CineDataWorkerRole entry point called", "Information");

            TMDBService tmdb = new TMDBService();

            dtNextFullRun =  DateTime.MinValue;
            DateTime dtLastRun = DateTime.MinValue; //GetLastModified();
            
            if (dtLastRun.Date < DateTime.UtcNow.Date)
                dtNextFullRun = DateTime.UtcNow;
            else
                dtNextFullRun = DateTime.UtcNow.Date.AddHours(26);

            int tenMinSleepDuration = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;

            LoadLocationData();

            while (true)
            {
                Trace.WriteLine("Working", "Information");
                
                if (DateTime.UtcNow >= dtNextFullRun)
                {
                    // execute a full run

                    dtNextFullRun = DateTime.UtcNow.Date.AddHours(26);

                    Cineworld.Configuration conf = null;
                    Task<Cineworld.Configuration> tConf = tmdb.GetConfig();
                    tConf.Wait();

                    if (!tConf.IsFaulted && tConf.Result != null)
                        conf = tConf.Result;
                    else
                        continue;

                    dictionaryCinemaFilms.Clear();

                    FilmsUK.Clear();
                    FilmsIE.Clear();

                    CinemasUK.Clear();
                    CinemasIE.Clear();

                    cinemaFilmsUK.Clear();
                    filmCinemasUK.Clear();

                    cinemaFilmsIE.Clear();
                    filmCinemasIE.Clear();

                    bool processedUKFilms = false;
                    bool processedUKCinemas = false;
                    bool processedIEFilms = false;
                    bool processedIECinemas = false;
                    bool cinemafilmSaved = false;
                    HashSet<int> ukCinemaPerformances = new HashSet<int>();
                    HashSet<int> ieCinemaPerformances = new HashSet<int>();

                    while (true)
                    {
                        try
                        {
                            if (!processedUKFilms)
                            {
                                ProcessFilmInfo(conf, FilmsUK, filmCinemasUK, RegionDef.GB);
                                processedUKFilms = true;
                            }

                            if (!processedUKCinemas)
                            {
                                ProcessCinemaInfo(CinemasUK, cinemaFilmsUK, FilmsUK, RegionDef.GB);
                                processedUKCinemas = true;
                            }

                            if (!processedIEFilms)
                            {
                                ProcessFilmInfo(conf, FilmsIE, filmCinemasIE, RegionDef.IE, true);
                                processedIEFilms = true;
                            }

                            if (!processedIECinemas)
                            {
                                ProcessCinemaInfo(CinemasIE, cinemaFilmsIE, FilmsIE, RegionDef.IE);
                                processedIECinemas = true;
                            }

                            if (!cinemafilmSaved)
                            {
                                ProcessData();
                                cinemafilmSaved = true;
                            }

                            ProcessPerformances(ukCinemaPerformances, ieCinemaPerformances);

                            break;
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, null);
                            Thread.Sleep(tenMinSleepDuration);
                        }
                    }
                }
                
#if DEBUG
                return;
#else
                Thread.Sleep(tenMinSleepDuration);
#endif
            }
        }

        private void LoadLocationData()
        {
            #region build longitude dictionary

            this.LongitudeDictionary.Add(1, -2.077981);
            this.LongitudeDictionary.Add(2,	0.570953);
            this.LongitudeDictionary.Add(3, -0.100704);
            this.LongitudeDictionary.Add(4,	-2.587152);
            this.LongitudeDictionary.Add(5,	-1.632478);
            this.LongitudeDictionary.Add(6,	0.706591);
            this.LongitudeDictionary.Add(7,	0.136949);
            this.LongitudeDictionary.Add(8,	-3.173408);
            this.LongitudeDictionary.Add(9,	-1.341912);
            this.LongitudeDictionary.Add(10, -0.173564);
            this.LongitudeDictionary.Add(11, -2.075038);
            this.LongitudeDictionary.Add(12, 0.871624);
            this.LongitudeDictionary.Add(14, -1.424461);
            this.LongitudeDictionary.Add(15, -0.784824);
            this.LongitudeDictionary.Add(16, -0.189657);
            this.LongitudeDictionary.Add(17, -1.239876);
            this.LongitudeDictionary.Add(18, -2.220455);
            this.LongitudeDictionary.Add(19, -3.04621);
            this.LongitudeDictionary.Add(20, 0.322835);
            this.LongitudeDictionary.Add(21, -3.216954);
            this.LongitudeDictionary.Add(22, -0.061231);
            this.LongitudeDictionary.Add(23, -2.111594);
            this.LongitudeDictionary.Add(24, -3.779923);
            this.LongitudeDictionary.Add(25, -0.40646);
            this.LongitudeDictionary.Add(26, -0.17944);
            this.LongitudeDictionary.Add(27, -4.199841);
            this.LongitudeDictionary.Add(28, -4.255196);
            this.LongitudeDictionary.Add(30, -0.233318);
            this.LongitudeDictionary.Add(31, 0.108052);
            this.LongitudeDictionary.Add(32, -0.132406);
            this.LongitudeDictionary.Add(33, -0.756225);
            this.LongitudeDictionary.Add(34, -0.445875);
            this.LongitudeDictionary.Add(35, -0.353009);
            this.LongitudeDictionary.Add(36, -0.180713);
            this.LongitudeDictionary.Add(37, 0.074101);
            this.LongitudeDictionary.Add(38, 1.150987);
            this.LongitudeDictionary.Add(39, -1.289241);
            this.LongitudeDictionary.Add(40, -3.9);
            this.LongitudeDictionary.Add(41, -2.922903);
            this.LongitudeDictionary.Add(42, -3.807231);
            this.LongitudeDictionary.Add(43, -0.417672);
            this.LongitudeDictionary.Add(44, -1.226278);
            this.LongitudeDictionary.Add(45, 0.150026);
            this.LongitudeDictionary.Add(46, -0.74871);
            this.LongitudeDictionary.Add(47, -2.943595);
            this.LongitudeDictionary.Add(48, -0.93661);
            this.LongitudeDictionary.Add(49, -1.14982);
            this.LongitudeDictionary.Add(50, 0.477071);
            this.LongitudeDictionary.Add(51, -1.259195);
            this.LongitudeDictionary.Add(52, -2.699983);
            this.LongitudeDictionary.Add(53, -0.133024);
            this.LongitudeDictionary.Add(54, -1.414981);
            this.LongitudeDictionary.Add(55, -2.740545);
            this.LongitudeDictionary.Add(56, -1.914684);
            this.LongitudeDictionary.Add(57, -1.779402);
            this.LongitudeDictionary.Add(58, -1.393437);
            this.LongitudeDictionary.Add(59, -2.737958);
            this.LongitudeDictionary.Add(60, -0.229728);
            this.LongitudeDictionary.Add(61, -0.208269);
            this.LongitudeDictionary.Add(62, -2.160928);
            this.LongitudeDictionary.Add(63, -1.831346);
            this.LongitudeDictionary.Add(64, -1.50612);
            this.LongitudeDictionary.Add(65, -0.194205);
            this.LongitudeDictionary.Add(66, -0.023856);
            this.LongitudeDictionary.Add(67, -1.466334);
            this.LongitudeDictionary.Add(68, -2.455131);
            this.LongitudeDictionary.Add(69, -2.093214);
            this.LongitudeDictionary.Add(70, -0.106469);
            this.LongitudeDictionary.Add(71, -2.625539);
            this.LongitudeDictionary.Add(72, -2.423972);
            this.LongitudeDictionary.Add(73, -1.747772);
            this.LongitudeDictionary.Add(76, 0.439967);
            this.LongitudeDictionary.Add(77, -1.486073);
            this.LongitudeDictionary.Add(78, -2.096921);
            this.LongitudeDictionary.Add(79, 0.005519);
            this.LongitudeDictionary.Add(83, -2.515175);
            this.LongitudeDictionary.Add(87, -0.765172);
            this.LongitudeDictionary.Add(88, -4.293805);
            this.LongitudeDictionary.Add(89, -0.284271);
            this.LongitudeDictionary.Add(90, -2.252966);
            this.LongitudeDictionary.Add(75, -6.2622309);
            
            #endregion

            #region build latitude dictionary

            this.LatitudeDictionary.Add(1, 57.150283);
            this.LatitudeDictionary.Add(2, 51.86999);
            this.LatitudeDictionary.Add(3, 50.812648);
            this.LatitudeDictionary.Add(4, 51.416356);
            this.LatitudeDictionary.Add(5, 52.805604);
            this.LatitudeDictionary.Add(6, 52.24621);
            this.LatitudeDictionary.Add(7, 52.190173);
            this.LatitudeDictionary.Add(8, 51.478744);
            this.LatitudeDictionary.Add(9, 53.710182);
            this.LatitudeDictionary.Add(10, 51.485344);
            this.LatitudeDictionary.Add(11, 51.902934);
            this.LatitudeDictionary.Add(12, 51.161429);
            this.LatitudeDictionary.Add(14, 53.227039);
            this.LatitudeDictionary.Add(15, 50.829998);
            this.LatitudeDictionary.Add(16, 51.120245);
            this.LatitudeDictionary.Add(17, 51.608173);
            this.LatitudeDictionary.Add(18, 53.407997);
            this.LatitudeDictionary.Add(19, 56.4844);
            this.LatitudeDictionary.Add(20, 50.794864);
            this.LatitudeDictionary.Add(21, 55.941947);
            this.LatitudeDictionary.Add(22, 51.650918);
            this.LatitudeDictionary.Add(23, 53.489045);
            this.LatitudeDictionary.Add(24, 56.003781);
            this.LatitudeDictionary.Add(25, 51.443598);
            this.LatitudeDictionary.Add(26, 51.487162);
            this.LatitudeDictionary.Add(27, 55.853449);
            this.LatitudeDictionary.Add(28, 55.864912);
            this.LatitudeDictionary.Add(30, 51.492536);
            this.LatitudeDictionary.Add(31, 51.783887);
            this.LatitudeDictionary.Add(32, 51.508813);
            this.LatitudeDictionary.Add(33, 51.630815);
            this.LatitudeDictionary.Add(34, 52.133321);
            this.LatitudeDictionary.Add(35, 53.792495);
            this.LatitudeDictionary.Add(36, 52.35142);
            this.LatitudeDictionary.Add(37, 51.557645);
            this.LatitudeDictionary.Add(38, 52.053232);
            this.LatitudeDictionary.Add(39, 50.698972);
            this.LatitudeDictionary.Add(40, 54.455);
            this.LatitudeDictionary.Add(41, 53.408408);
            this.LatitudeDictionary.Add(42, 53.282972);
            this.LatitudeDictionary.Add(43, 51.881858);
            this.LatitudeDictionary.Add(44, 54.574665);
            this.LatitudeDictionary.Add(45, 51.456286);
            this.LatitudeDictionary.Add(46, 52.041437);
            this.LatitudeDictionary.Add(47, 51.577578);
            this.LatitudeDictionary.Add(48, 52.235228);
            this.LatitudeDictionary.Add(49, 52.955413);
            this.LatitudeDictionary.Add(50, 51.380042);
            this.LatitudeDictionary.Add(51, 52.385234);
            this.LatitudeDictionary.Add(52, 53.326192);
            this.LatitudeDictionary.Add(53, 51.510963);
            this.LatitudeDictionary.Add(54, 53.401529);
            this.LatitudeDictionary.Add(55, 52.703543);
            this.LatitudeDictionary.Add(56, 52.474403);
            this.LatitudeDictionary.Add(57, 52.412413);
            this.LatitudeDictionary.Add(58, 50.895618);
            this.LatitudeDictionary.Add(59, 53.449811);
            this.LatitudeDictionary.Add(60, 51.570302);
            this.LatitudeDictionary.Add(61, 51.899915);
            this.LatitudeDictionary.Add(62, 53.406915);
            this.LatitudeDictionary.Add(63, 51.560807);
            this.LatitudeDictionary.Add(64, 53.677158);
            this.LatitudeDictionary.Add(65, 51.455413);
            this.LatitudeDictionary.Add(66, 51.507655);
            this.LatitudeDictionary.Add(67, 54.946795);
            this.LatitudeDictionary.Add(68, 50.610246);
            this.LatitudeDictionary.Add(69, 52.596281);
            this.LatitudeDictionary.Add(70, 51.594555);
            this.LatitudeDictionary.Add(71, 50.940378);
            this.LatitudeDictionary.Add(72, 53.597194);
            this.LatitudeDictionary.Add(73, 53.791618);
            this.LatitudeDictionary.Add(76, 52.083659);
            this.LatitudeDictionary.Add(77, 51.787157);
            this.LatitudeDictionary.Add(78, 57.143568);
            this.LatitudeDictionary.Add(79, 51.501581);
            this.LatitudeDictionary.Add(83, 53.494161);
            this.LatitudeDictionary.Add(87, 51.249224);
            this.LatitudeDictionary.Add(88, 55.85832);
            this.LatitudeDictionary.Add(89, 51.557362);
            this.LatitudeDictionary.Add(90, 51.860518);
            this.LatitudeDictionary.Add(75, 53.3522644);

            #endregion 
        }

        private void LogError(Exception ex, string message)
        {
            //ConfigurationManager.ConnectionStrings["CineStorageConStr"].ConnectionString
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CineStorageConStr"));

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = blobClient.GetContainerReference("data");

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob logBlob = container.GetBlockBlobReference(String.Format(this.LogFile, DateTime.UtcNow.ToString("dd MMM yyyy")));
            try
            {
                using (StreamReader sr = new StreamReader(logBlob.OpenRead()))
                {
                    using (StreamWriter sw = new StreamWriter(logBlob.OpenWrite()))
                    {
                        sw.Write(sr.ReadToEnd());

                        if (ex != null)
                        {
                            sw.Write(System.Environment.NewLine);

                            sw.WriteLine(ex.Message);
                            sw.WriteLine(ex.StackTrace);

                            sw.Write(System.Environment.NewLine);

                            if (ex.InnerException != null)
                            {
                                sw.Write(System.Environment.NewLine);

                                sw.WriteLine(ex.InnerException.Message);
                                sw.WriteLine(ex.InnerException.StackTrace);

                                sw.Write(System.Environment.NewLine);
                            }
                        }

                        if (message != null)
                        {
                            sw.Write(System.Environment.NewLine);

                            sw.WriteLine(message);
                            
                            sw.Write(System.Environment.NewLine);
                        }
                    }
                }
            }
            catch { }
        }
        
        private DateTime GetLastModified()
        {
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CineStorageConStr"));

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = blobClient.GetContainerReference("data");

            // Retrieve reference to a blob.
            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cinemasUKBlob = container.GetBlockBlobReference(CinemasUKFileName);
            if (cinemasUKBlob.Exists())
            {
                DateTimeOffset? dto = cinemasUKBlob.Properties.LastModified;
                if (dto.HasValue)
                    return dto.Value.DateTime;
            }
            
            return DateTime.MinValue;
        }

        private void ProcessData()
        {
            //return;

            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CineStorageConStr"));
            
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = blobClient.GetContainerReference("data");

            // Retrieve reference to a blob.
            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cinemasUKBlob = container.GetBlockBlobReference(CinemasUKFileName);
            using (Stream s = cinemasUKBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(CinemasUK));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob filmsUKBlob = container.GetBlockBlobReference(FilmsUKFileName);
            using (Stream s = filmsUKBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {

                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(FilmsUK));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob filmCinemasUKBlob = container.GetBlockBlobReference(FilmCinemasUKFileName);
            using (Stream s = filmCinemasUKBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(filmCinemasUK));
                    }       
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cinemaFilmsUKBlob = container.GetBlockBlobReference(CinemaFilmsUKFileName);
            using (Stream s = cinemaFilmsUKBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(cinemaFilmsUK));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cinemasIEBlob = container.GetBlockBlobReference(CinemasIEFileName);
            using (Stream s = cinemasIEBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(CinemasIE));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob filmsIEBlob = container.GetBlockBlobReference(FilmsIEFileName);
            using (Stream s = filmsIEBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(FilmsIE));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob filmCinemasIEBlob = container.GetBlockBlobReference(FilmCinemasIEFileName);
            using (Stream s = filmCinemasIEBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(filmCinemasIE));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cinemaFilmsIEBlob = container.GetBlockBlobReference(CinemaFilmsIEFileName);
            using (Stream s = cinemaFilmsIEBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(cinemaFilmsIE));
                    }
                }
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob filmsPostersBlob = container.GetBlockBlobReference(MovieFilmPostersFileName);
            using (Stream s = filmsPostersBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        sw.Write(JsonConvert.SerializeObject(MoviePosters));
                    }
                }
            }

            //Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob mediumfilmsPostersBlob = container.GetBlockBlobReference(MediumFilmPostersFileName);
            //using (Stream s = mediumfilmsPostersBlob.OpenWrite())
            //{
            //    using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
            //    {
            //        using (StreamWriter sw = new StreamWriter(gzipStream))
            //        {
            //            sw.Write(JsonConvert.SerializeObject(mediumposters));
            //        }
            //    }
            //}
        }

        private void ProcessCinemaInfo(Dictionary<int, CinemaInfo> currentCinemas, Dictionary<int, List<FilmInfo>> cinemaFilms, Dictionary<int, FilmInfo> Films, RegionDef region)
        {
            CineworldService cws = new CineworldService();
            Cinemas cinemas = null;
            Task<Cinemas> tcinemas = cws.GetCinemas(region, true);
            tcinemas.Wait();

            if (!tcinemas.IsFaulted && tcinemas.Result != null)
                cinemas = tcinemas.Result;

            CineMobileService mobileService = new CineMobileService();
            
            //foreach (var cinema in cinemas.cinemas)
            for(int i = 0; i < cinemas.cinemas.Count; i++)
            {
                //if (i > 1)
                //    break;

                var cinema = cinemas.cinemas[i];

                //if (cinema.id != 91)
                //    continue;

                Task<Films> tf = cws.GetFilms(region, false, cinema.id);

                Task<List<CinemaReview>> tReviews = mobileService.GetCinemaReviews(cinema.id);

                if (!currentCinemas.ContainsKey(cinema.id))
                {
                    CinemaInfo ci = new CinemaInfo()
                    {
                        Address = cinema.address,
                        ID = cinema.id,
                        Name = cinema.name,
                        Postcode = cinema.postcode,
                        Telephone = cinema.telephone
                    };

                    if (this.LatitudeDictionary.ContainsKey(cinema.id) && this.LongitudeDictionary.ContainsKey(cinema.id))
                    {
                        ci.Latitude = this.LatitudeDictionary[cinema.id];
                        ci.Longitute = this.LongitudeDictionary[cinema.id];
                    }
                    else
                    {
                        Bing.MapsClient mapClient = new Bing.MapsClient("At7qhfJw20G5JptEm0fdIaMehzBAU6GT4jJRpznGY_rdPRa5NquCN5GP8bzzdG0d", "en-GB");
                        var tLocation = mapClient.LocationQuery(
                            new Bing.Maps.Address()
                            {
                                AddressLine = cinema.address,
                                PostalCode = cinema.postcode,
                                CountryRegion = (region == RegionDef.GB ? "United Kingdom" : "Ireland")
                            });

                        tLocation.Wait();

                        Bing.Maps.Location location = null;

                        try
                        {
                            if (!tLocation.IsFaulted && tLocation.Result != null)
                            {
                                location = tLocation.Result.GetLocations().FirstOrDefault();
                            }
                        }
                        catch { }

                        if (location != null && location.Point != null && location.Point.Coordinates.Length == 2)
                        {
                            ci.Latitude = location.Point.Coordinates[0];
                            ci.Longitute = location.Point.Coordinates[1];
                        }
                    }

                    tReviews.Wait();
                    if (!tReviews.IsFaulted && tReviews.Result != null)
                    {
                        ci.Reviews = tReviews.Result;
                    }

                    currentCinemas[cinema.id] =  ci;
                }

                tf.Wait();

                if (!tf.IsFaulted && tf.Result != null)
                {
                    List<FilmInfo> cFilms = new List<FilmInfo>();
                    foreach (var film in tf.Result.films)
                    {
                        if (!Films.ContainsKey(film.edi))
                            continue;

                        FilmInfo fi = new FilmInfo() { EDI = film.edi, Title = film.CleanTitle };
                        fi.ShowingToday = true;

                        cFilms.Add(fi);
                    }
                    cinemaFilms[cinema.id] = cFilms;
                }
            }
        }

        private void ProcessPerformances(HashSet<int> ukCinemaPerformances, HashSet<int> ieCinemaPerformances)
        {
            foreach (var cinemafilms in this.cinemaFilmsUK)
            {
                if (!ukCinemaPerformances.Contains(cinemafilms.Key))
                {
                    try
                    {
                        Task t = ProcessCinemaPerformances(RegionDef.GB, cinemafilms.Key, cinemafilms.Value);
                        t.Wait();

                        ukCinemaPerformances.Add(cinemafilms.Key);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Error processing UK cinema id " + cinemafilms.Key);
                    }
                }
            }

            foreach (var cinemafilms in this.cinemaFilmsIE)
            {
                if (!ieCinemaPerformances.Contains(cinemafilms.Key))
                {
                    try
                    {
                        Task t = ProcessCinemaPerformances(RegionDef.IE, cinemafilms.Key, cinemafilms.Value);
                        t.Wait();

                        ieCinemaPerformances.Add(cinemafilms.Key);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Error processing IE cinema id " + cinemafilms.Key);
                    }
                }
            }
        }

        public async Task ProcessCinemaPerformances(RegionDef region, int cinemaID, List<FilmInfo> films)
        {
            CineworldService cws = new CineworldService();
            List<FilmHeader> filmsForCinema = new List<FilmHeader>();
            foreach (var film in films)
            {
                Task<Dates> td = cws.GetDates(region, cinemaID, film.EDI);
                td.Wait();

                HashSet<DateTime> perfDates = new HashSet<DateTime>();
                if (!td.IsFaulted && td.Result != null && td.Result.dates != null)
                {
                    FilmHeader fh = new FilmHeader() { EDI = film.EDI };
                    filmsForCinema.Add(fh);

                    fh.Performances = new List<PerformanceInfo>();
                    
                    foreach (var date in td.Result.dates)
                    {
                        DateTime performanceDate = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);

                        Task<Performances> tp = cws.GetPerformances(region, cinemaID, film.EDI, date);
                        tp.Wait();

                        if (!tp.IsFaulted && tp.Result != null && tp.Result.performances != null)
                        {
                            foreach (var p in tp.Result.performances)
                            {
                                PerformanceInfo pi = new PerformanceInfo();
                                pi.PerformanceTS = performanceDate.Add(DateTime.ParseExact(p.time, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay);
                                pi.Available = p.available;
                                pi.Type = p.Type;
                                pi.BookUrl = new Uri(p.booking_url);
                                fh.Performances.Add(pi);
                            }
                        }
                    }
                }
            }

            //return;

            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CineStorageConStr"));

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = blobClient.GetContainerReference("data");

            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cinemaFilmsBlob = container.GetBlockBlobReference(String.Format(FilmsPerCinemaFileName, cinemaID));
            using (Stream s = cinemaFilmsBlob.OpenWrite())
            {
                using (var gzipStream = new GZipStream(s, CompressionMode.Compress))
                {
                    using (StreamWriter sw = new StreamWriter(gzipStream))
                    {
                        await sw.WriteAsync(JsonConvert.SerializeObject(filmsForCinema));
                    }
                }
            }
        }

        

        private void ProcessFilmInfo(Cineworld.Configuration config, Dictionary<int, FilmInfo> currentFilms, Dictionary<int, List<CinemaInfo>> filmCinemas, RegionDef region, bool searchTMDB = true)
        {
            CineworldService cws = new CineworldService();
            Films films = null;
            Task<Films> tfilms = cws.GetFilms(region, true);
            tfilms.Wait();

            if(!tfilms.IsCanceled && tfilms.Result != null)
                films = tfilms.Result;

            TMDBService tmdb = new TMDBService();

            HashSet<int> newFilmIds = new HashSet<int>();

            CineMobileService mobileService = new CineMobileService();
            
            //foreach (var film in films.films)
            for(int i = 0; i < films.films.Count; i++)
            {
                //if (i > 0)
                //    break;

                var film = films.films[i];

                newFilmIds.Add(film.edi);

                //if (film.edi != 128529 && film.edi != 139438)
                //    continue;

                Task<Cinemas> tcinemas = cws.GetCinemas(region, false, int.MinValue, film.edi);
                
                if (!currentFilms.ContainsKey(film.edi) || !currentFilms[film.edi].TMDBDataLoaded)
                {
                    Movie movieData = null;
                    Task<FilmImages> tmovieImages = null;

                    FilmInfo filminfo = new FilmInfo() { EDI = film.edi, Classification = !String.IsNullOrWhiteSpace(film.classification) ? film.classification : "TBC", Title = film.ProcessedTitle, TMDBDataLoaded = false };

                    if (movieData == null)
                    {
                        Results res = null;
                        try
                        {
                            DateTime ukRelease = film.GetReleaseDate();
                            Task<Results> tres = tmdb.GetSearchResults(film.CleanTitle, ukRelease);
                            tres.Wait();
                            if (!tres.IsFaulted && tres.Result != null)
                            {
                                res = tres.Result;
                            }

                            if (res.SearchResults == null || res.SearchResults.Count == 0 && ukRelease != DateTime.MinValue)
                            {
                                Task<Results> tres2 = tmdb.GetSearchResults(film.CleanTitle, DateTime.MinValue);
                                tres2.Wait();
                                if (!tres2.IsFaulted && tres2.Result != null)
                                {
                                    res = tres2.Result;
                                }
                            }

                            if (res.SearchResults != null && res.SearchResults.Count > 0)
                            {
                                Result movieSearchRes = res.SearchResults.First();

                                Task<Movie> tmovieData = tmdb.GetMovieDetails(movieSearchRes.Id);
                                tmovieData.Wait();

                                tmovieImages = tmdb.GetFilmImages(movieSearchRes.Id);

                                if (!tmovieData.IsFaulted && tmovieData.Result != null)
                                {
                                    movieData = tmovieData.Result;
                                }

                                if (movieData.Trailers != null && movieData.Trailers.Youtube != null && movieData.Trailers.Youtube.Count > 0 && !String.IsNullOrEmpty(movieData.Trailers.Youtube[0].Source))
                                {
                                    if (movieData.Trailers.Youtube[0].Source.IndexOf('=') == -1)
                                    {
                                        string[] trailerparts = movieData.Trailers.Youtube[0].Source.Split('=');
                                        movieData.YouTubeTrailerID = trailerparts[trailerparts.Length - 1];
                                    }
                                    else
                                    {
                                        movieData.YouTubeTrailerID = movieData.Trailers.Youtube[0].Source;
                                    }
                                }
                                else
                                {
                                    Task<string> tyoutube = (new YouTubeService().GetVideoId(film.CleanTitle));
                                    tyoutube.Wait();

                                    if (!tyoutube.IsCanceled && tyoutube.Result != null)
                                        movieData.YouTubeTrailerID = tyoutube.Result;
                                }

                                if (ukRelease == DateTime.MinValue)
                                {
                                    Country c = movieData.Releases.Countries.FirstOrDefault(country => String.Compare(country.Iso31661, region == RegionDef.GB ? "GB" : "IE", StringComparison.OrdinalIgnoreCase) == 0);
                                    if (c != null)
                                    {
                                        ukRelease = DateTime.ParseExact(c.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    }
                                    else if (!String.IsNullOrEmpty(movieSearchRes.ReleaseDate))
                                    {
                                        ukRelease = DateTime.ParseExact(movieSearchRes.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    }
                                }

                                movieData.CineworldReleaseDate = ukRelease;
                            }
                        }
                        catch { }
                    }
 
                    if (movieData != null)
                    {
                        filminfo.TMDBDataLoaded = true;

                        filminfo.TmdbId = movieData.Id;
                        filminfo.Overview = movieData.Overview;
                        filminfo.Tagline = movieData.Tagline;
                        filminfo.Runtime = movieData.Runtime;
                        filminfo.Genres = new List<string>();
                        if (movieData.Genres != null)
                        {
                            foreach (var g in movieData.Genres)
                                filminfo.Genres.Add(g.Name);
                        }

                        if(tmovieImages != null)
                        {
                            tmovieImages.Wait();

                            if(!tmovieImages.IsFaulted && tmovieImages.Result != null)
                            {
                                if (tmovieImages.Result.Backdrops != null)
                                {
                                    foreach (var backdrop in tmovieImages.Result.Backdrops)
                                    {
                                        string url = string.Format("{0}{1}{2}", config.Images.BaseUrl, ConfigurationManager.AppSettings["mediumposterwidth"], backdrop.FilePath);
                                        filminfo.Backdrops.Add(new Uri(url));
                                    }
                                }

                                if (tmovieImages.Result.Posters != null)
                                {
                                    foreach (var poster in tmovieImages.Result.Posters)
                                    {
                                        string url = string.Format("{0}{1}{2}", config.Images.BaseUrl, ConfigurationManager.AppSettings["mediumposterwidth"], poster.FilePath);
                                        filminfo.Posters.Add(new Uri(url));
                                    }
                                }
                            }
                        }
                        
                        if (!String.IsNullOrEmpty(movieData.PosterPath))
                        {
                            string poster = string.Format("{0}{1}{2}", config.Images.BaseUrl, ConfigurationManager.AppSettings["posterwidth"], movieData.PosterPath);

                            string mediumposter = string.Format("{0}{1}{2}", config.Images.BaseUrl, ConfigurationManager.AppSettings["mediumposterwidth"], movieData.PosterPath);

                            if (!MoviePosters.ContainsKey(filminfo.TmdbId))
                            {
                                MoviePosters.Add(filminfo.TmdbId, mediumposter);
                            }

                            filminfo.PosterUrl = new Uri(poster);
                            filminfo.MediumPosterUrl = new Uri(mediumposter);
                        }
                        else if (!String.IsNullOrEmpty(film.poster_url))
                        {
                            filminfo.PosterUrl = new Uri(film.poster_url);
                        }

                        if (!String.IsNullOrEmpty(movieData.BackdropPath))
                        {
                            string backdrop = string.Format("{0}{1}{2}", config.Images.BaseUrl, ConfigurationManager.AppSettings["backdropwidth"], movieData.BackdropPath);
                            filminfo.BackdropUrl = new Uri(backdrop);
                        }

                        filminfo.YoutubeTrailer = movieData.YouTubeTrailerID;

                        filminfo.Release = movieData.CineworldReleaseDate;

                        if (movieData.Casts != null && movieData.Casts.Cast != null)
                        {
                            foreach (var cast in movieData.Casts.Cast)
                            {
                                filminfo.FilmCast.Add(new CastInfo()
                                {
                                    ID = cast.Id,
                                    Name = cast.Name,
                                    Character = cast.Character,
                                    ProfilePath = (!String.IsNullOrEmpty(cast.ProfilePath) ? new Uri(string.Format("{0}{1}{2}", config.Images.BaseUrl, ConfigurationManager.AppSettings["posterwidth"], cast.ProfilePath)) : null)
                                });
                            }
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(film.poster_url))
                        {
                            filminfo.PosterUrl = new Uri(film.poster_url);
                        }
                    }

                    Task<List<FilmReview>> tReviews = mobileService.GetFilmReviews(filminfo);
                
                    tReviews.Wait();

                    if (!tReviews.IsFaulted && tReviews.Result != null)
                        filminfo.Reviews = tReviews.Result;

                    currentFilms[filminfo.EDI] = filminfo;
                }

                tcinemas.Wait();

                if (!tcinemas.IsFaulted && tcinemas.Result != null)
                {
                    List<CinemaInfo> fCinemas = new List<CinemaInfo>();
                    foreach (var cinema in tcinemas.Result.cinemas)
                        fCinemas.Add(new CinemaInfo() { ID = cinema.id, Name = cinema.name });

                    filmCinemas[film.edi] = fCinemas;
                }
            }

            HashSet<int> currentFilmIds = new HashSet<int>(currentFilms.Keys);
            currentFilmIds.ExceptWith(newFilmIds);

            foreach (var id in currentFilmIds)
            {
                currentFilms.Remove(id);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}
