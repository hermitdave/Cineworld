using Cineworld;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
public class FilmHeader
{
    public int EDI { get; set; }
    public List<PerformanceInfo> Performances { get; set; }
}