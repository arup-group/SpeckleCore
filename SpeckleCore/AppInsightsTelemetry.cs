using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCore
{
    public class AppInsightsTelemetry
    {

        public static void Track()
        {
            // you may use different options to create configuration as shown later in this article
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            var telemetryClient = new TelemetryClient(configuration);
            telemetryClient.TrackTrace("Hello World!");
        }

    }
}
