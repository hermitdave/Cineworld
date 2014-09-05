using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
#if WINDOWS_PHONE && !AGENT
using System.IO.IsolatedStorage;
using System.Net.Http;
using Microsoft.Xna.Framework.Media;
#elif AGENT
using System.IO.IsolatedStorage;
using System.Net.Http;
#else
using System.Net.Http;
using System.IO;
#endif
#if NETFX_CORE
using Windows.Storage;
#endif

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

                    httpRequstMessage.Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
                    httpRequstMessage.Headers.Add("Accept-Language", "en-GB,en;q=0.5");
                    httpRequstMessage.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
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
                
#if WINDOWS_PHONE

                    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                    string file = (folder == null ? filename : String.Format("{0}/{1}", folder, filename));
                
                    using (Stream slocal = isf.OpenFile(file, FileMode.Create))
                    
#elif NETFX_CORE
                    StorageFile sf = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                    
                    using (Stream slocal = await sf.OpenStreamForWriteAsync())
#else
                    Stream slocal = null;
#endif
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
#if AGENT

#elif WINDOWS_PHONE
                    await s.CopyToAsync(ms);
                    
#elif NETFX_CORE
                    
#else
                    
#endif
                    
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
