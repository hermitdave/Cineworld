using Cineworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
#if WINDOWS_PHONE
using System.Device.Location;
using System;
#endif

public class CinemaInfo
{
    public CinemaInfo()
    {
        this.Reviews = new List<CinemaReview>();
    }

    public int ID { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Postcode { get; set; }
    public string Telephone { get; set; }

    [JsonIgnore]
    public string FullAddress  
    {
        get { return String.Format("{0} {1}", Address, Postcode); }
    }

    [JsonIgnore]
    public string Region { get { return this.Name.Substring(0, 1); } }

    public double Longitute { get; set; }
    public double Latitude { get; set; }

    public List<FilmInfo> Films { get; set; }

    public List<CinemaReview> Reviews { get; set; }

    [JsonIgnore]
    public double AverageRating
    {
        get
        {
            double total = 0;

            if (this.VoteCount == 0)
                return 0;

                foreach (var review in this.Reviews)
                total += review.Rating;

            return total / this.Reviews.Count;
        }
    }

    [JsonIgnore]
    public int VoteCount
    {
        get { return this.Reviews == null ? 0 : this.Reviews.Count; }
    }

#if WINDOWS_PHONE
    [JsonIgnore]
    public GeoCoordinate Location
    {
        get { return new GeoCoordinate(this.Latitude, this.Longitute); }
    }
#endif
}