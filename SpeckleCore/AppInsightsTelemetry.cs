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

        public static void Track(SpeckleApiClient speckleApiClient)
        {
            // you may use different options to create configuration as shown later in this article
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            var telemetryClient = new TelemetryClient(configuration);
            telemetryClient.TrackTrace("Hello World!");

            PageViewTelemetry telemetry = new PageViewTelemetry();
            telemetry.Properties.Add("client", speckleApiClient.ClientType);
            telemetry.Properties.Add("os_version", TelemetryUtilities.OsVersion);
            telemetry.Properties.Add("speckle_version", TelemetryUtilities.SpeckleCoreVersion);
            telemetry.Properties.Add("user", speckleApiClient.User._id);
            telemetry.Properties.Add("user_is_owner", speckleApiClient.IsStreamOwner());
            
            telemetry.Name = "Stream-Send";
            telemetryClient.TrackPageView(telemetry);
        }

    }
}
