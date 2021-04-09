using Piwik.Tracker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public static void Initialize()
        {
            piwikTracker = new PiwikTracker(SiteId, PiwikBaseUrl);
            piwikTracker.SetUserId(ComputeSHA256Hash(Environment.UserName + "@" + internalDomain + ".com"));
        }

        public static void SendEvent(string category, string action, string name = "", string value = "")
        {
            if (piwikTracker == null)
                Initialize();

            piwikTracker.DoTrackEvent(category, action, name, value);
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
