using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;

namespace Cineworld
{
    internal class AsyncWebClient
    {
        public async Task<string> GetStringAsync(string url)
        {
            string data = null;
            while (true)
            {
                try
                {
					var httpClient = new HttpClient();
					var httpRequstMessage = new HttpRequestMessage(HttpMethod.Get, url);
					//httpRequstMessage.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
                    //httpRequstMessage.Headers.Add("Accept-Language", "en-GB,en;q=0.5");
                    //httpRequstMessage.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
					var response = await httpClient.SendAsync(httpRequstMessage);

                    data = await response.Content.ReadAsStringAsync();
                    break;
                }
                catch
                {
                }
            }

            return data;
        }

        public async Task DownloadFileAsync(string url, string filename, string folder = null)
        {
            var httpClient = new HttpClient();
            
            try
            {
                using (Stream s = await httpClient.GetStreamAsync(url))
                {
					using(Stream slocal = new FileStream(Path.Combine(folder, filename), FileMode.Create))
                    {
                        await s.CopyToAsync(slocal);
                    }
                }
            }
            catch (Exception ex)
            {
                if (1 == 1)
                {
                }
            }
        }


        public async Task<byte[]> SavePictureLocally(string url, Stream ms)
        {
            var httpClient = new HttpClient();

            try
            {
                using (Stream s = await httpClient.GetStreamAsync(url))
                {
                    await s.CopyToAsync(ms);                    
                }
            }
            catch (Exception ex)
            {
                if (1 == 1)
                {
                }
            }

            return null;
        }
    }
}
