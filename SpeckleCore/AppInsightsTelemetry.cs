using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCore
{
    public static class AppInsightsTelemetry
    {
        public static string OsVersion { get; private set; }
        public static string UserAgent { get; private set; }
        public static string SpeckleCoreVersion { get; private set; }

        private static TelemetryClient telemetryClient;
        public static TelemetryClient TelemetryClient
        {
            get
            {
                if (telemetryClient == null)
                    telemetryClient = GetTelemetryClient();
                return telemetryClient;
            }
        }

        public static void TrackWithMetaAppInsights(this SpeckleApiClient speckleApiClient, string trackName)
        {
            var metrics = new Dictionary<string, double>()
            {
                {"object_num", speckleApiClient.GetNumberOfObjects() },
            };

            var properties = speckleApiClient.GetTrackClientProperties();

            TrackCustomAppInsights(trackName, metrics, properties);
        }

        private static TelemetryClient GetTelemetryClient()
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            // you may use different options to create configuration as shown later in this article
            configuration.InstrumentationKey = "932d9212-7677-4d4c-8494-10176c2cdf25";
            var telemetryClient = new TelemetryClient(configuration);
            //telemetryClient.TrackTrace("Hello World!");
            return telemetryClient;
        }

        public static void TrackCustomAppInsights(string trackName, Dictionary<string, double> metrics, Dictionary<string, string> properties)
        {
            if (!LocalContext.GetTelemetrySettings())
                return;

            var telemetryClient = TelemetryClient;
            PageViewTelemetry telemetry = new PageViewTelemetry();
            telemetry.Name = trackName;

            foreach (var prop in properties)
            {
                telemetry.Properties.Add(prop);
            }

            foreach (var metric in metrics)
            {
                telemetry.Metrics.Add(metric);
            }

            telemetryClient.TrackPageView(telemetry);
        }

    }
}
