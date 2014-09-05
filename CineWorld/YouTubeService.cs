using System;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cineworld;

namespace Cineworld
{
    public class YouTubeService
    {
        string youtubeUri = "http://gdata.youtube.com/feeds/api/videos?q={0} official trailer&max-results=1&v=2&alt=jsonc&format=5&fmt=18";

        public async Task<Uri> GetVideoUrl(string title)
        {
            string queryUrl = String.Format(youtubeUri, title);

            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(queryUrl);

            VideoSearchResults v = JsonConvert.DeserializeObject<VideoSearchResults>(data);

            if (v != null && v.Data != null && v.Data.Items != null && v.Data.Items.Count > 0)
            {
#if WINDOWS_PHONE
                return null;
#elif METRO
                var uri = await MyToolkit.Multimedia.YouTube.GetVideoUriAsync(v.Data.Items[0].Id, MyToolkit.Multimedia.YouTubeQuality.Quality1080P);

                if (uri.IsValid)
                    return uri.Uri;
#endif
                //return String.Format("vnd.youtube:{0}", v.Data.Items[0].Id);
            }

            return null;
        }

        public async Task<string> GetVideoId(string title)
        {
            string queryUrl = String.Format(youtubeUri, title);

            AsyncWebClient awc = new AsyncWebClient();
            string data = await awc.GetStringAsync(queryUrl);

            VideoSearchResults v = JsonConvert.DeserializeObject<VideoSearchResults>(data);

            if (v != null && v.Data != null && v.Data.Items != null && v.Data.Items.Count > 0)
            {
                return v.Data.Items[0].Id;
            }

            return null;
        }
    }
}
