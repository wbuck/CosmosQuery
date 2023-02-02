using Microsoft.OData.UriParser;

namespace CosmosQuery.Query;

internal record QueryOptions(OrderByClause OrderByClause, int? Skip, int? Top);