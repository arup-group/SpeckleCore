using Piwik.Tracker;
using System.Linq;

namespace SpeckleCore
{
    public static class MatomoTelemetry
    {
        private const string PiwikBaseUrl = "https://arupdt.matomo.cloud/";
        private const int SiteId = 4;
        private const string internalDomain = "arup";

        private static PiwikTracker piwikTracker;

        private static void Initialize(SpeckleApiClient speckleApiClient)
        {
            piwikTracker = new PiwikTracker(SiteId, PiwikBaseUrl);
            piwikTracker.SetUserId(TelemetryUtilities.ComputeSHA256Hash(speckleApiClient.User._id)); //Environment.UserName + "@" + internalDomain + ".com"
        }

        public static void StreamSend(this SpeckleApiClient speckleApiClient)
        {
            SendEvent(speckleApiClient, "stream", "send", "object_num", speckleApiClient.GetNumberOfObjects().ToString());
        }

        public static void StreamReceive(this SpeckleApiClient speckleApiClient)
        {
            SendEvent(speckleApiClient, "stream", "receive", "object_num", speckleApiClient.GetNumberOfObjects().ToString());
        }

        public static int GetNumberOfObjects(this SpeckleApiClient speckleApiClient)
        {
            return (int)speckleApiClient.Stream.Layers.Select(x => x.ObjectCount).Sum();
        }


        public static void SendEvent(this SpeckleApiClient speckleApiClient, string category, string action, string name = "", string value = "")
        {
            if (!LocalContext.GetTelemetrySettings())
                return;
            if (piwikTracker == null)
                Initialize(speckleApiClient);
            piwikTracker.AddCustomParametersFromSpeckle(speckleApiClient);
            piwikTracker.DoTrackEvent(category, action, name, value);
            AppInsightsTelemetry.Track(speckleApiClient);
        }

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
