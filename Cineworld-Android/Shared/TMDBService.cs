using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cineworld;

namespace Cineworld
{
    public class TMDBService
    {
        public const string APIKey = "d1aa1f599dc6b8b9f6db08b8270a3a62"; // set the api key

        public const string BaseAPIUrl = "http://api.themoviedb.org/3/";

        public const string SearchMovieApiUrl = "search/movie";
        public const string MovieDetailsApiUrl = "movie/";
        public const string PersonDetailsApiUrl = "person/";
        public const string ConfigApiUrl = "configuration";
        public const string ImagesApiUrl = "images";

        public const string AppKeyTag = "api_key";
        public const string QueryTag = "query";
        public const string YearTag = "year";

        public async Task<Configuration> GetConfig()
        {
            string url = String.Format("{0}{1}?{2}={3}", BaseAPIUrl, ConfigApiUrl, AppKeyTag, APIKey);

            //webclient.DownloadStringAsync(new Uri(url, UriKind.Absolute));
            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(url);

            Configuration r = JsonConvert.DeserializeObject<Configuration>(data);

            return r;
        }

#if AZURE
        public async Task<Results> GetSearchResults(string title, DateTime dtRelease)
        {
            string url = null;

            DateTime release = dtRelease;

            bool bTryEarlier = false;
            bool bSecondTry = false;
            Results r = null;

            while(true)
            {
                if(dtRelease != DateTime.MinValue)
                {
                    url = String.Format("{0}{1}?{2}={3}&{4}={5}&{6}={7}", BaseAPIUrl, SearchMovieApiUrl, AppKeyTag, APIKey, QueryTag, title, YearTag, release.Year);
                    if(!bSecondTry)
                        bTryEarlier = true;
                }
                else
                    url = String.Format("{0}{1}?{2}={3}&{4}={5}", BaseAPIUrl, SearchMovieApiUrl, AppKeyTag, APIKey, QueryTag, title);

                //webclient.DownloadStringAsync(new Uri(url, UriKind.Absolute));
                AsyncWebClient awc = new AsyncWebClient();
                string data = await awc.GetStringAsync(url);

                r = JsonConvert.DeserializeObject<Results>(data);

                if (bTryEarlier && dtRelease != DateTime.MinValue && (r.SearchResults == null || r.SearchResults.Count == 0))
                {
                    release = dtRelease.AddYears(-1);
                    bSecondTry = true;
                    bTryEarlier = false;
                }
                else
                    break;
            }

            return r;
        }

        public async Task<Movie> GetMovieDetails(int movieID)
        {
            string url = String.Format("{0}{1}{2}?{3}={4}&append_to_response=releases,trailers,poster,casts", BaseAPIUrl, MovieDetailsApiUrl, movieID, AppKeyTag, APIKey);

            //webclient.DownloadStringAsync(new Uri(url, UriKind.Absolute));
            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(url);

            Movie m = JsonConvert.DeserializeObject<Movie>(data);

            return m;
        }

        public async Task<FilmImages> GetFilmImages(int filmID)
        {
            string url = String.Format("{0}{1}{2}/{3}?{4}={5}&append_to_response=credits", BaseAPIUrl, MovieDetailsApiUrl, filmID, ImagesApiUrl, AppKeyTag, APIKey);

            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(url);

            FilmImages i = JsonConvert.DeserializeObject<FilmImages>(data);

            return i;
        }

#else
        public async Task<Person> GetPersonDetails(int personID)
        {
            string url = String.Format("{0}{1}{2}?{3}={4}&append_to_response=credits", BaseAPIUrl, PersonDetailsApiUrl, personID, AppKeyTag, APIKey);

            //webclient.DownloadStringAsync(new Uri(url, UriKind.Absolute));
            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(url);

            Person p = JsonConvert.DeserializeObject<Person>(data);

            return p;
        }
#endif


    }
}
