using Cineworld;
//using MyToolkit.Multimedia;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class FilmInfo
{
    private static string[] arIdentifiers = { "2D", "3D", "IMAX", "Autism Friendly Screening", "Hindi Version", "Tamil", "Polish Language without subtitles", "with English Subtitles", "In Spanish with English subtitles", "Cineworld Unlimited Exclusive Show" };

    [JsonIgnore]
    public string CleanTitle
    {

        get
        {
            string lowercaseTitle = this.Title.ToLower();
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
            if (res.IndexOf('/') > -1)
            {
                res = res.Remove(res.Length - 10, 10).Trim();
            }

            return res;
        }
    }
        
    public FilmInfo() 
    {
        FilmCast = new List<CastInfo>();
        ShowingAtCinemas = new List<CinemaInfo>();
        Reviews = new List<FilmReview>();
        Genres = new List<string>();
        Backdrops = new List<Uri>();
        Posters = new List<Uri>();
    }

    public int EDI { get; set; }
    public int TmdbId { get; set; }

    public string Title { get; set; }
    public DateTime Release { get; set; }
    public string Tagline { get; set; }
    public string Overview { get; set; }
    public string Classification { get; set; }
    public int Runtime { get; set; }

    public Uri PosterUrl { get; set; }
    public Uri MediumPosterUrl { get; set; }
    public Uri BackdropUrl { get; set; }

    public string YoutubeTrailer { get; set; }

    public List<Uri> Backdrops { get; set; }
    public List<Uri> Posters { get; set; }

    public List<FilmReview> Reviews { get; set; }

    public List<CinemaInfo> ShowingAtCinemas { get; set; }
    public List<CastInfo> FilmCast { get; set; }
    public List<String> Genres { get; set; }

    public string GenresCSV { get { return this.Genres == null ? String.Empty : String.Join<string>(", ", this.Genres); } }

    [JsonIgnore]
    public bool TMDBDataLoaded { get; set; }

    [JsonIgnore]
    public string ReleaseDate
    {
        get
        {
            return this.Release == DateTime.MinValue ? "unknown" : this.Release.ToString("dd MMM yyyy");
        }
    }

    [JsonIgnore]
    public string TitleWithClassification
    {
        get
        {
            return String.IsNullOrWhiteSpace(this.Classification) ? this.Title : String.Format("{0} ({1})", this.Title, this.Classification);
        }
    }

    [JsonIgnore]
    public string ShortDesc
    {
        get
        {
            if (String.IsNullOrWhiteSpace(this.Overview))
                return String.Empty;
            else if(this.Overview.Length <= 140)
                return this.Overview;
            else
                return String.Format("{0}..", this.Overview.Substring(0, 140));
        }
    }

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
    

    [JsonIgnore]
    public Uri PosterImage
    {
        get { return this.MediumPosterUrl == null ? this.PosterUrl : this.MediumPosterUrl; }
    }

    [JsonIgnore]
    public List<PerformanceInfo> Performances { get; set; }

    [JsonIgnore]
    public List<PerformanceInfo> PerformancesPrimary
    {
        get
        {
            if (this.Performances == null)
                return null;

            if (this.Performances.Count <= 6)
                return this.Performances;

            return this.Performances.Take(6).ToList();
        }
    }

    [JsonIgnore]
    public List<PerformanceInfo> PerformancesSecondary
    {
        get
        {
            if (this.Performances == null)
                return null;

            if (this.Performances.Count <= 6)
                return null;

            return this.Performances.Skip(6).ToList();
        }
    }

#if AZURE
    public bool ShowingToday { get; set; }
#else
    [JsonIgnore]
    public bool ShowingToday
    {
        get
        {
            if (this.Performances != null && this.Performances.Count > 0)
            {
#if WINDOWS_PHONE
                var perf = this.Performances.FirstOrDefault(p => p.PerformanceTS.Date == DateTime.UtcNow.Date);
#else
                var perf = this.Performances.Find(p => p.PerformanceTS.Date == DateTime.UtcNow.Date);
#endif
                if (perf != null)
                    return true;
            }

            return false;
        }
    }

    [JsonIgnore]
    public bool ShowingTomorrow
    {
        get
        {
            if (this.Performances != null && this.Performances.Count > 0)
            {
#if WINDOWS_PHONE
                var perf = this.Performances.FirstOrDefault(p => p.PerformanceTS.Date == DateTime.UtcNow.Date.AddDays(1));
#else
                var perf = this.Performances.Find(p => p.PerformanceTS.Date == DateTime.UtcNow.Date.AddDays(1));
#endif
                if (perf != null)
                    return true;
            }

            return false;
        }
    }
#endif
    
    [JsonIgnore]
    public char HeaderChar 
    {
        get
        {
            char c = Char.ToUpperInvariant(Title[0]);
            int iChar = (int)c;

            if (iChar >= 65 && iChar <= 90)
                return c;
            else
                return '#';

        }
    }

    public FilmInfo Clone()
    {
        return (FilmInfo)JsonConvert.DeserializeObject<FilmInfo>(JsonConvert.SerializeObject(this));
    }
}
