using Piwik.Tracker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            piwikTracker.SetUserId(ComputeSHA256Hash(speckleApiClient.User._id)); //Environment.UserName + "@" + internalDomain + ".com"
        }

        public static void StreamSend(this SpeckleApiClient speckleApiClient)
        {
            SendEvent(speckleApiClient, "stream", "send", "object_num", speckleApiClient.Stream.Objects.Count.ToString());
        }

        public static void StreamReceive(this SpeckleApiClient speckleApiClient)
        {
            SendEvent(speckleApiClient, "stream", "receive", "object_num", speckleApiClient.Stream.Objects.Count.ToString());
        }

        public static void SendEvent(this SpeckleApiClient speckleApiClient, string category, string action, string name = "", string value = "")
        {
            //if(!speckleApiClient.CanTrack)
            // return;
            if (piwikTracker == null)
                Initialize(speckleApiClient);

            piwikTracker.AddCustomParametersFromSpeckle(speckleApiClient);
            piwikTracker.DoTrackEvent(category, action, name, value);
        }

        public static void AddCustomParametersFromSpeckle(this PiwikTracker piwikTracker, SpeckleApiClient speckleApiClient)
        {
            piwikTracker.SetCustomTrackingParameter("server_name", speckleApiClient.BaseUrl);
            piwikTracker.SetCustomTrackingParameter("client", speckleApiClient.ClientType);
            piwikTracker.SetCustomTrackingParameter("speckle_version", Assembly.GetEntryAssembly().GetName().Version.ToString());
            piwikTracker.SetCustomTrackingParameter("user", speckleApiClient.User._id);
            piwikTracker.SetCustomTrackingParameter("user_is_creator", speckleApiClient.User._id == speckleApiClient.Stream.Owner? "True" : "False");
        }

        private static string ComputeSHA256Hash(string text)
        {
            using (var sha256 = new SHA256Managed())
            {
                byte[] _encodedText = Encoding.UTF8.GetBytes(text);
                byte[] _hash = sha256.ComputeHash(_encodedText);
                string _hashString = BitConverter.ToString(_hash);
                return _hashString.Replace("-", "").ToLower(new CultureInfo("en-GB", false));
            }
        }
    }
}
