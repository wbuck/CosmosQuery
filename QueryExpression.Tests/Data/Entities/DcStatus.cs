using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QueryExpression.Tests.Data.Entities;

[JsonConverter(typeof(StringEnumConverter))]
public enum DcStatus
{
    Healthy,
    NotHealthy
}
