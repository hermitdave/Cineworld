using Newtonsoft.Json;
using System;
public class CastInfo
{
    public int ID { get; set; }

    public string Name { get; set; }

    public string Character { get; set; }

    public Uri ProfilePath { get; set; }

    public Uri ProfilePicture
    {
        get
        {
            if (ProfilePath != null)
                return ProfilePath;
            else            
                return new Uri("PlaceHolder.png", UriKind.Relative);
        }
    }

    public string Title
    {
        get 
        {
            return String.IsNullOrEmpty(this.Character) ? String.Format("{0}\n ", this.Name) : String.Format("{0}\nas {1}", this.Name, this.Character);
        }
    }
}