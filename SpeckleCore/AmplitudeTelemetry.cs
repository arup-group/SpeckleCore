using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SpeckleCore
{
    public static class AmplitudeTelemetry
    {
        const string requestUri = "https://api.amplitude.com/2/httpapi";
        const string api_key = "b0c9d54ebd92a1e6be5197ef7de42c00";

        public static void TrackWithMetaAmplitude(this SpeckleApiClient speckleApiClient, string trackName)
        {
            if (!LocalContext.GetTelemetrySettings())
                return;

            try
            {
                var properties = speckleApiClient.GetTrackClientProperties();
                properties.Add("object_num", speckleApiClient.GetNumberOfObjects().ToString());
            var user_email = TelemetryUtilities.ComputeSHA256Hash(speckleApiClient.User.Email);
            TrackAmplitudeAsync(requestUri, api_key, user_email, properties, trackName);
            }
            catch { }
        }

        public static void TrackCustomAmplitude(string trackName, string user_id, Dictionary<string, string> user_properties)
        {
            if (!LocalContext.GetTelemetrySettings())
                return;

            TrackAmplitudeAsync(requestUri, api_key, user_id, user_properties, event_type: trackName);
        }

        public static async void TrackAmplitudeAsync(string requestUri, string api_key, string user_id, Dictionary<string, string> user_properties, string event_type = "", string ip = "")
        {
            try
            {
                HttpWebRequest request = PrepRequest(requestUri, api_key, user_id, user_properties, event_type, ip);
                var response = await request.GetResponseAsync().ConfigureAwait(false);
                var resp = (HttpWebResponse)response;
            }
            catch { }
        }

        public static void TrackAmplitude(string requestUri, string api_key, string user_id, Dictionary<string, string> user_properties, string event_type = "", string ip = "")
        {
            try
            {
                HttpWebRequest request = PrepRequest(requestUri, api_key, user_id, user_properties, event_type, ip);

                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("Response: " + resp.StatusCode.ToString());
                }
            }
            catch { }
        }

        private static HttpWebRequest PrepRequest(string requestUri, string api_key, string user_id, Dictionary<string, string> user_properties, string event_type, string ip)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "POST";

            var myEvent = new AmplitudeBody
            {
                api_key = api_key,
                events = new AmplitudeEvent[]
                {
                    new AmplitudeEvent
                    {
                        user_id = user_id,
                        event_type = event_type,
                        user_properties = user_properties,
                        ip = ip
                    }
                }
            };

            var body = JsonConvert.SerializeObject(myEvent);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(body);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byte1.Length;
            using (var newStream = request.GetRequestStream())
            {
                newStream.Write(byte1, 0, byte1.Length);
                return request;
            }
        }

        public class AmplitudeBody
        {
            public string api_key { get; set; }
            public AmplitudeEvent[] events { get; set; }
        }

        public class AmplitudeEvent
        {
            public string user_id { get; set; }
            public string event_type { get; set; }
            public string country { get; set; }
            public string ip { get; set; }
            public Dictionary<string, string> user_properties { get; set; }

        }

    }
}
