using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SpeckleCore
{

    public static class TelemetryUtilities
    {
        private static string osVersion = string.Empty;
        private static string userAgent = string.Empty;

        public static string OsVersion
        {
            get
            {
                if (osVersion.Equals(string.Empty))
                    SetOsRelatedData();
                return osVersion;
            }
        }

        public static string UserAgent
        {
            get
            {
                if (userAgent.Equals(string.Empty))
                    SetOsRelatedData();
                return userAgent;
            }
        }

        private static string speckleCoreVersion = string.Empty;

        public static string SpeckleCoreVersion
        {
            get
            {
                if (speckleCoreVersion.Equals(string.Empty))
                    speckleCoreVersion = GetSpeckleVersion();
                return speckleCoreVersion;
            }
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
            userAgent = info.Properties["Caption"].Value.ToString();
            osVersion = info.Properties["Version"].Value.ToString();
        }

        public static string ComputeSHA256Hash(string text)
        {
            using (var sha256 = new SHA256Managed())
            {
                byte[] _encodedText = Encoding.UTF8.GetBytes(text);
                byte[] _hash = sha256.ComputeHash(_encodedText);
                string _hashString = BitConverter.ToString(_hash);
                return _hashString.Replace("-", "").ToLower(new CultureInfo("en-GB", false));
            }
        }
        private static string GetSpeckleVersion()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.Where(x => x.FullName.Contains("SpeckleCore")).FirstOrDefault();
            var version = assembly.GetName().Version;
            return version.ToString();
        }
    
        public static string IsStreamOwner(this SpeckleApiClient speckleApiClient)
        {
            return speckleApiClient.User._id == speckleApiClient.Stream.Owner ? "True" : "False";
        }

        public static void Track(this SpeckleApiClient speckleApiClient, string trackName)
        {
            if (!LocalContext.GetTelemetrySettings())
                return;
            speckleApiClient.TrackWithMetaMatomo(trackName, "");
            speckleApiClient.TrackWithMetaAppInsights(trackName);
        }

        public static void TrackCustom(string trackName, Dictionary<string, double> metrics, Dictionary<string, string> properties)
        {
            if (!LocalContext.GetTelemetrySettings())
                return;
            AppInsightsTelemetry.TrackCustomAppInsights(trackName, metrics, properties);
            MatomoTelemetry.TrackCustomMatomo(trackName, "", "", "", properties);
        }

        public static void StreamSend(this SpeckleApiClient speckleApiClient)
        {
            MatomoTelemetry.TrackWithMetaMatomo(speckleApiClient, "stream", "send", "object_num", speckleApiClient.GetNumberOfObjects().ToString());
            speckleApiClient.TrackWithMetaAppInsights("stream-send");
        }

        public static void StreamReceive(this SpeckleApiClient speckleApiClient)
        {
            MatomoTelemetry.TrackWithMetaMatomo(speckleApiClient, "stream", "receive", "object_num", speckleApiClient.GetNumberOfObjects().ToString());
            speckleApiClient.TrackWithMetaAppInsights("stream-receive");
        }

        public static Dictionary<string, string> GetTrackClientProperties(this SpeckleApiClient speckleApiClient)
        {
            return new Dictionary<string, string>()
            {
                {"client", speckleApiClient.ClientType},
                { "os_version", TelemetryUtilities.OsVersion},
                { "speckle_version", TelemetryUtilities.SpeckleCoreVersion},
                { "user", speckleApiClient.User._id},
                { "user_is_owner", speckleApiClient.IsStreamOwner()},
            };
        }

        public static int GetNumberOfObjects(this SpeckleApiClient speckleApiClient)
        {
            SpeckleStream stream = speckleApiClient.Stream;
            if (stream.Layers != null)
                return (int)stream.Layers.Select(x => x.ObjectCount).Sum();
            else
                return stream.Objects != null ? stream.Objects.Count : 0;
        }

    }
}
