#if WINDOWS_PHONE
using Microsoft.Phone.Info;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cineworld
{
    public class ExtendedPropertyHelper
    {
        private static readonly int ANIDLength = 32;  
        private static readonly int ANIDOffset = 2;

        // NOTE: to get a result requires ID_CAP_IDENTITY_USER  
        //  to be added to the capabilities of the WMAppManifest  
        // this will then warn users in marketplace  
        public static string GetWindowsLiveAnonymousID()
        {
            string result = string.Empty;
            
            object anid;

#if WINDOWS_PHONE && !WP8
            if (UserExtendedProperties.TryGetValue("ANID", out anid))
#elif WINDOWS_PHONE && WP8
            if (UserExtendedProperties.TryGetValue("ANID2", out anid))
#else
            anid = null;
#endif
            {
                if (anid != null && anid.ToString().Length >= (ANIDLength + ANIDOffset))
                {
                    result = anid.ToString().Substring(ANIDOffset, ANIDLength);
                }
            }
            return result;
        }

        public static string GetUserIdentifier()
        {
            string id = null;
#if WINDOWS_PHONE
            id = GetWindowsLiveAnonymousID();

            if (String.IsNullOrEmpty(id))
                id = Config.UserGuid;
#else
            id = Config.UserGuid;
#endif

            return id;
        }
    }
}
