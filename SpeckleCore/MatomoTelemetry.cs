using Piwik.Tracker;
using System.Collections.Generic;
using System.Linq;

namespace SpeckleCore
{
    public static class MatomoTelemetry
    {
        private const string PiwikBaseUrl = "https://arupdt.matomo.cloud/";
        private const int SiteId = 4;
        private const string internalDomain = "arup";

        private static PiwikTracker piwikTracker;

        private static void Initialize()
        {
            piwikTracker = new PiwikTracker(SiteId, PiwikBaseUrl);
        }

        public static void TrackWithMetaMatomo(this SpeckleApiClient speckleApiClient, string category, string action, string name = "", string value = "")
        {
            if (piwikTracker == null)
                Initialize();
            try
            {
                piwikTracker.SetUserId(TelemetryUtilities.ComputeSHA256Hash(speckleApiClient.User.Email));
                var properties = speckleApiClient.GetTrackClientProperties();
                TrackCustomMatomo(category, action, name, value, properties);
            }
            catch { }
        }

        public static void TrackCustomMatomo(string category, string action, string name = "", string value = "", Dictionary<string, string> properties = null)
        {
            if (!LocalContext.GetTelemetrySettings())
                return;
            try
            {
                if (piwikTracker == null)
                    Initialize();

                piwikTracker.SetUserAgent(TelemetryUtilities.UserAgent);

                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        piwikTracker.SetCustomTrackingParameter(prop.Key, prop.Value);
                    }
                }
                piwikTracker.DoTrackEvent(category, action, name, value);
            }
            catch { }
        }

        // It is only kept for the notes
        public static void AddCustomParametersFromSpeckle(this PiwikTracker piwikTracker, SpeckleApiClient speckleApiClient)
        {
            piwikTracker.SetUserAgent(TelemetryUtilities.UserAgent);

            piwikTracker.SetCustomTrackingParameter("client", speckleApiClient.ClientType);
            // Here are some variations that are suggested from the docs in order to track custom dimensions.
            // Will need to came back and clean-up here once we know how this works
            piwikTracker.SetCustomTrackingParameter("dimension1", speckleApiClient.ClientType);
            piwikTracker.SetCustomTrackingParameter("dimension2", TelemetryUtilities.OsVersion);
            piwikTracker.SetCustomTrackingParameter("os_version", TelemetryUtilities.OsVersion);
            piwikTracker.SetCustomTrackingParameter("speckle_version", TelemetryUtilities.SpeckleCoreVersion);
            piwikTracker.SetCustomTrackingParameter("user", speckleApiClient.User._id);
            piwikTracker.SetCustomTrackingParameter("user_is_owner", speckleApiClient.User._id == speckleApiClient.Stream.Owner ? "True" : "False");
            // Not sure if we need the line below for tracking custom Dimensions. 
            // or the one we are calling after this method is enough: piwikTracker.DoTrackEvent(category, action, name, value);
            //piwikTracker.DoTrackPageView("Record Metadata");
        }
    }


}
