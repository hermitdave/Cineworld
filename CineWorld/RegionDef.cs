using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
using Cineworld;
using System.Reflection;
#else
using Windows.Storage;
#endif

namespace Cineworld
{
    public enum AvailableCountries
    {
        UK,
        Ireland,
    }
}
