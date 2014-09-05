using Cineworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PerformanceInfo
{
    public DateTime PerformanceTS { get; set; }
    public bool Available { get; set; }
    public Uri BookUrl { get; set; }


    public string Type { get; set; }

    [JsonIgnore]
    public Uri MobileBookingUrl
    {
        get
        {
            if (BookUrl == null) return null;

            string Url = BookUrl.OriginalString.Replace(".co.uk", ".co.uk/mobile");

            return new Uri(Url, UriKind.Absolute);
        }
    }

    [JsonIgnore]
    public string FilmTitle { get; set; }

    [JsonIgnore]
    public string TimeString
    {
        get
        {
            return this.PerformanceTS.ToString("HH:mm");
        }
    }

    [JsonIgnore]
    public bool AvailableFuture
    {
        get
        {
            return this.Available && (DateTime.Now < this.PerformanceTS || DateTime.Now.Subtract(this.PerformanceTS).TotalMinutes <= 20);
        }
    }
}
