using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
#if WINDOWS_PHONE
using CineWorld;
#elif NETFX_CORE
using Cineworld;
#endif

namespace Cineworld
{
    public class ReviewBase
    {
        public long Id { get; set; }
        public short Rating { get; set; }
        public string Review { get; set; }
        public DateTime ReviewTS { get; set; }
        public string Reviewer { get; set; }
        public string UserId { get; set; }

        public string ReviewTimeStamp
        {
            get { return this.ReviewTS.ToString("dd MMM yyyy"); }
        }
    }
    
    public class FilmReview : ReviewBase
    {
        public FilmReview()
        {
        }

        public int Movie { get; set; }

        public int TmdbId { get; set; }
    }

    public class CinemaReview : ReviewBase
    {
        public CinemaReview()
        {
        }

        public int Cinema { get; set; }
    }

    public class BookingHistory
    {
        public long Id { get; set; }
        public int PerformanceId { get; set; }
        public DateTime PerformanceTS { get; set; }
        public string UserId { get; set; }

        static char[] cSplitters = { '=', '&' };

        public BookingHistory(PerformanceInfo pi)
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (App.MobileService == null)
                App.MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");
#endif
            string[] parts = pi.BookUrl.Query.Split(cSplitters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                try
                {
                    this.PerformanceId = int.Parse(parts[1]);
                }
                catch { }
            }

            this.PerformanceTS = pi.PerformanceTS;
        }
    }
}