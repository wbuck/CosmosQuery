using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CosmosQuery.Benchmarks.Data.Entities;

[JsonConverter(typeof(StringEnumConverter))]
public enum DcStatus
{
    Healthy,
    NotHealthy
}
