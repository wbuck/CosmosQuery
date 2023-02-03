using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CosmosQuery.Tests.Entities;

[JsonConverter(typeof(StringEnumConverter))]
public enum DcStatus
{
    Healthy,
    NotHealthy
}
