using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CineWorld
{
    public class CustomUriMapper : UriMapperBase
    {
        public const string UriSchemaTag = "cineworld";
        public const string FilmTag = "film";
        public const string CinemaTag = "cinema";
        public const string SearchTag = "search";

        
        public override Uri MapUri(Uri uri)
        {
            string tempUri = HttpUtility.UrlDecode(uri.ToString()).ToLowerInvariant();
            if(tempUri.Contains(UriSchemaTag))
            {
                int pos = tempUri.LastIndexOf('?');
                if (pos > -1)
                {
                    string paramValue = tempUri.Substring(pos + 1);
                        
                    if (tempUri.Contains(FilmTag))
                    {
                        return new Uri(String.Format("/FilmDetails.xaml?FilmID={0}", paramValue), UriKind.Relative);
                    }
                    else if (tempUri.Contains(SearchTag))
                    {
                        return new Uri(String.Format("/Search.xaml?Find={0}", paramValue), UriKind.Relative);
                    }
                    else if (tempUri.Contains(CinemaTag))
                    {
                        return new Uri(String.Format("/CinemaDetails.xaml?CinemaID={0}", paramValue), UriKind.Relative);
                    }
                }
            }

            return uri;
        }
    }
}
