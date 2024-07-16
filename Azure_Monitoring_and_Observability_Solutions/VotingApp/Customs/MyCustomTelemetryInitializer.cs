using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
internal class MyCustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        // Replace with actual properties.
        (telemetry as ISupportProperties).Properties["app-member-id"] = "SD2990"; //replace with your sd here
    }
}

