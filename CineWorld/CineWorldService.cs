using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Cineworld
{
    public class Films
    {
        public List<Film> films;
        public int Cinema;
    }

    public class Film
    {
        private static string[] arIdentifiers = { "2D", "3D", "IMAX", "Autism Friendly Screening", "Hindi Version", "Tamil", "Polish Language without subtitles", "with English Subtitles", "In Spanish with English subtitles", "Cineworld Unlimited Exclusive Show" };
        public int edi;
        public string Title { get { return title; } }

        public DateTime GetReleaseDate()
        {
            if (String.IsNullOrEmpty(this.film_url))
                return DateTime.MinValue;

            string url = this.film_url.Replace("cineworld.co.uk/", "cineworld.co.uk/mobile/");

            string day = null;
            string month = null;
            string year = null;

            AsyncWebClient awc = new AsyncWebClient();

            string htmlData = null;

            while (true)
            {
                try
                {
                    Task<string> tFilmData = awc.GetStringAsync(url);

                    tFilmData.Wait();

                    if (!tFilmData.IsCanceled && tFilmData.Result != null)
                    {
                        htmlData = tFilmData.Result;
                        break;
                    }
                }
                catch { }
            }

            if (htmlData != null)
            {
                using (StringReader sr = new StringReader(htmlData.ToLower()))
                {
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.IndexOf("release date") > -1)
                        {
                            char[] aSplit = { ' ' };
                            string strippedData = Regex.Replace(line, @"<(.|\n)*?>", string.Empty);
                            string[] parts = strippedData.Split(aSplit, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length < 5)
                                return DateTime.MinValue;

                            day = parts[parts.Length - 3];
                            month = parts[parts.Length - 2];
                            year = parts[parts.Length - 1];
                            break;
                        }
                    }
                }
            }

            return year == null ? DateTime.MinValue : DateTime.Parse(string.Format("{0} {1} {2}", day, month, year));
        }

        public string CleanTitle
        {
     
            get
            {
                string lowercaseTitle = title.ToLower();
                StringBuilder sb = new StringBuilder(lowercaseTitle);
                foreach (var sId in arIdentifiers)
                {
                    var replace = sId.ToLower();

                    if (lowercaseTitle.IndexOf(replace) > -1)
                    {
                        sb.Replace(replace, "");
                    }
                }
                sb.Replace(":", "");
                sb.Replace("-", "");
                sb.Replace("(", "");
                sb.Replace(")", "");

                string res = sb.ToString().Trim();
                //if (res.IndexOf('/') > -1)
                //{
                //    res = res.Remove(res.Length - 10, 10).Trim();
                //}
                
                return res;
            }
        }

        public string ProcessedTitle
        {

            get
            {
                StringBuilder sb = new StringBuilder(title);
                foreach (var sId in arIdentifiers)
                {
                    if (title.IndexOf(sId, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        sb.Replace(sId, "");
                        sb.AppendFormat(" {0}", sId);
                    }
                }
                
                sb.Replace(":", "");
                sb.Replace("-", "");
                sb.Replace("(", "");
                sb.Replace(")", "");

                string res = sb.ToString().Trim();

                if (res[0] == ':')
                {
                    res = res.Remove(0, 1).Trim();
                }
                
                //if (res.IndexOf('/') > -1 && res.Length >= 10)
                //{
                //    res = res.Remove(res.Length - 10, 10).Trim();
                //}

                return res;
            }
        }

        public string GroupingChar
        {
            get
            {
                return this.ProcessedTitle[0].ToString();
            }
        }

        public string title;
        public int id;
        public string classification;
        public string advisory;
        public string Poster { get { return poster_url; } }
        public string poster_url;
        public string still_url;
        public string film_url;
        public string Description { get; set; }
        public string TrailerUri { get; set; }

        public Dates Dates { get; set; }
    }

    public class Cinemas
    {
        public List<Cinema> cinemas;
    }

    public class Cinema
    {
        public int id;
        public string Name { get { return name; } }
        public string name;

        public string cinema_url;
        public string Address { get { return address; } }
        public string address;
        public string postcode;
        public string telephone;

        public string Region
        {
            get
            {
                return this.name[0].ToString();
            }
        }
    }

    public class Dates
    {
        public List<String> dates;

        public List<DateTime> PerformanceDates
        {
            get
            {
                List<DateTime> retCol = new List<DateTime>();
                foreach (var dt in dates)
                {
                    try
                    {
                        retCol.Add(DateTime.ParseExact(dt, "yyyyMMdd", CultureInfo.InvariantCulture));
                    }
                    catch { }
                }

                return retCol;
            }
        }

        public List<string> DateStrings
        {
            get
            {
                List<string> retCol = new List<string>();
                foreach (var dt in PerformanceDates)
                {
                    retCol.Add(dt.ToString("dd MMM yyyy"));
                }

                return retCol;
            }
        }
    }

    public class Performances
    {
        public DateTime Date { get; set; }

        public List<Performance> performances;
    }

    public class Performance
    {
        [JsonIgnore]
        public string Time { get { return time; } }
        
        public string time;

        public bool available;
        public string type;
        
        [JsonIgnore]
        public string Type
        {
            get
            {
                string retVal = null;
                switch (this.type)
                {
                    case "reg":
                        retVal = "Regular";
                        break;

                    case "vip":
                        retVal = "VIP";
                        break;

                    case "del":
                        retVal = "Delux";
                        break;

                    case "m4j":
                        retVal = "Juniors";
                        break;

                    default:
                        retVal = "D-Box";
                        break;
                }

                return retVal;
            }
        }
        public bool ad;
        public bool subtitled;

        [JsonIgnore]
        public string BookingUrl { get { return this.booking_url; } }
        
        public string booking_url;
    }

    public enum RegionDef
    {
        GB,
        IE,
    }
    
    public class CineworldService
    {
        
        public const string DevKey = "VR3kQprU"; // set the dev key

        public const string BaseAPIUrl = "http://46.248.186.122/api/quickbook/";

//#if IE
//        public const string CurrentTerritory = "IE";
//#else
//        public const string CurrentTerritory = "GB";
//#endif

        public const string PerformancesHandle = "performances";
        public const string DatesHandle = "dates";
        public const string CinemasHandle = "cinemas";
        public const string FilmsHandle = "films";

        public const string QSTerritoryParam = "territory";
        public const string QSKeyParam = "key";
        public const string QSCinemaParam = "cinema";
        public const string QSFilmParam = "film";
        public const string QSDateParam = "date";
        public const string QSFullParam = "full";

        public const string ErrorMessage = "Error downloading data";

        public async Task<Performances> GetPerformances(RegionDef region, int cinema, int film, string date)
        {
            string url = $"{BaseAPIUrl}{PerformancesHandle}?{QSKeyParam}={DevKey}&{QSCinemaParam}={cinema}&{QSFilmParam}={film}&{QSDateParam}={date}&{QSTerritoryParam}={region}";

            //webclient.DownloadStringAsync(new Uri(url, UriKind.Absolute));
            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(url);

            DateTime dt = DateTime.ParseExact(date, "yyyymmdd", CultureInfo.InvariantCulture);

            Performances p = JsonConvert.DeserializeObject<Performances>(data);
            p.Date = dt;

            return p;
        }

        public async Task<Dates> GetDates(RegionDef region, int cinema, int film)
        {
            string url = $"{BaseAPIUrl}{DatesHandle}?{QSKeyParam}={DevKey}&{QSCinemaParam}={cinema}&{QSFilmParam}={film}&{QSTerritoryParam}={region}";

            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(url);

            Dates d = JsonConvert.DeserializeObject<Dates>(data);

            return d;
        }

        //public async Task<Cinemas> GetCinemas(bool fullInfo = false, int cinema = int.MinValue, int film = int.MinValue)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat(BaseAPIUrl, CinemasHandle);
        //    sb.AppendFormat("?{0}={1}&{2}={3}", QSKeyParam, DevKey, QSTerritoryParam, CurrentTerritory);

        //    if (cinema != int.MinValue)
        //        sb.AppendFormat("&{0}={1}", QSCinemaParam, cinema);

        //    if (film != int.MinValue)
        //        sb.AppendFormat("&{0}={1}", QSFilmParam, film);

        //    sb.AppendFormat("&{0}={1}", QSFullParam, fullInfo);

        //    AsyncWebClient awc = new AsyncWebClient();
        //    string data = await awc.GetStringAsync(sb.ToString());


        //    Cinemas c = JsonConvert.DeserializeObject<Cinemas>(data);
        //    return c;
        //}

        public async Task<Cinemas> GetCinemas(RegionDef region, bool fullInfo = false, int cinema = int.MinValue, int film = int.MinValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{BaseAPIUrl}{CinemasHandle}");
            sb.Append($"?{QSKeyParam}={DevKey}&{QSTerritoryParam}={region}");

            if (cinema != int.MinValue)
                sb.Append($"&{QSCinemaParam}={cinema}");

            if (film != int.MinValue)
                sb.Append($"&{QSFilmParam}={film}");

            sb.Append($"&{QSFullParam}={fullInfo}");

            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(sb.ToString());


            Cinemas c = JsonConvert.DeserializeObject<Cinemas>(data);
            return c;
        }

        public async Task<Films> GetFilms(RegionDef region, bool fullInfo = false, int cinema = int.MinValue, int film = int.MinValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{BaseAPIUrl}{FilmsHandle}");
            sb.Append($"?{QSKeyParam}={DevKey}&{QSTerritoryParam}={region}");

            if (cinema != int.MinValue)
                sb.Append($"&{QSCinemaParam}={cinema}");

            if (film != int.MinValue)
                sb.Append($"&{QSFilmParam}={film}");

            sb.Append($"&{QSFullParam}={fullInfo}");

            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(sb.ToString());

            Films f = JsonConvert.DeserializeObject<Films>(data);
            f.Cinema = cinema;
            return f;
        }
    }
}
