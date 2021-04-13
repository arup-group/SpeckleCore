using Piwik.Tracker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
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
        public static string OsVersion { get; private set; }
        public static string UserAgent { get; private set; }
        public static string SpeckleCoreVersion { get; private set; }

        private static void Initialize(SpeckleApiClient speckleApiClient)
        {
            piwikTracker = new PiwikTracker(SiteId, PiwikBaseUrl);
            piwikTracker.SetUserId(ComputeSHA256Hash(speckleApiClient.User._id)); //Environment.UserName + "@" + internalDomain + ".com"
            SetOsRelatedData();
            SpeckleCoreVersion = GetSpeckleVersion();
        }

        public static void StreamSend(this SpeckleApiClient speckleApiClient)
        {
            SendEvent(speckleApiClient, "stream", "send", "object_num", speckleApiClient.GetNumberOfObjects().ToString());
            AppInsightsTelemetry.Track();
        }

        public static void StreamReceive(this SpeckleApiClient speckleApiClient)
        {
            SendEvent(speckleApiClient, "stream", "receive", "object_num", speckleApiClient.GetNumberOfObjects().ToString());
            AppInsightsTelemetry.Track();
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
        }

        public static void AddCustomParametersFromSpeckle(this PiwikTracker piwikTracker, SpeckleApiClient speckleApiClient)
        {
            piwikTracker.SetUserAgent(UserAgent);
            
            piwikTracker.SetCustomTrackingParameter("client", speckleApiClient.ClientType);
            // Here are some variations that are suggested from the docs in order to track custom dimensions.
            // Will need to came back and clean-up here once we know how this works
            piwikTracker.SetCustomTrackingParameter("dimension1", speckleApiClient.ClientType);
            piwikTracker.SetCustomTrackingParameter("dimension2", OsVersion);
            piwikTracker.SetCustomTrackingParameter("os_version", OsVersion);
            piwikTracker.SetCustomTrackingParameter("speckle_version", SpeckleCoreVersion);
            piwikTracker.SetCustomTrackingParameter("user", speckleApiClient.User._id);
            piwikTracker.SetCustomTrackingParameter("user_is_creator", speckleApiClient.User._id == speckleApiClient.Stream.Owner ? "True" : "False");
            // Not sure if we need the line below for tracking custom Dimensions. 
            // or the one we are calling after this method is enough: piwikTracker.DoTrackEvent(category, action, name, value);
            //piwikTracker.DoTrackPageView("Record Metadata");
        }

        private static string GetSpeckleVersion()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.Where(x => x.FullName.Contains("SpeckleCore")).FirstOrDefault();
            var version = assembly.GetName().Version;
            return version.ToString();
        }

        /// <summary>
        /// source: https://stackoverflow.com/questions/32415679/how-can-i-get-the-real-os-version-in-c
        /// </summary>
        /// <returns></returns>
        private static void SetOsRelatedData()
        {
            var query = "SELECT * FROM Win32_OperatingSystem";
            var searcher = new ManagementObjectSearcher(query);
            var info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            UserAgent = info.Properties["Caption"].Value.ToString();
            OsVersion = info.Properties["Version"].Value.ToString();
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
