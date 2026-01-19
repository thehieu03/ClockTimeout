using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace BuildingBlocks.Logging;

public sealed class ActivityTraceEnricher:ILogEventEnricher
{

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var act = Activity.Current;
        if (act is null) return;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("trace_id",act.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("span_id",act.SpanId.ToString()));
    }
}