using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;

public class FilterOutFastDependencyCallProcessor : ITelemetryProcessor
{
    private ITelemetryProcessor Next { get; set; }

    // next will point to the next TelemetryProcessor in the chain.
    public FilterOutFastDependencyCallProcessor(ITelemetryProcessor next)
    {
        this.Next = next;
    }

    public void Process(ITelemetry item)
    {
        // To filter out an item, return without calling the next processor.
        if (!OKtoSend(item)) { return; }
        ModifyItem(item);
        this.Next.Process(item);
    }

    // Example: replace with your own criteria.
    private bool OKtoSend(ITelemetry item)
    {
        var request = item as RequestTelemetry;

        if (request != null && request.Duration.TotalMilliseconds < 10)
        {
            return false;
        }

        return true;
    }
    private void ModifyItem(ITelemetry item)
    {
        //or track something like as slower than 10ms
        item.Context.Properties.Add("app-user", "cohuynhhuu");//replace with put your name here
    }
}