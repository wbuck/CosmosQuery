using Microsoft.OData.UriParser;

namespace CosmosQuery.Query;

internal record QueryOptions(OrderByClause Clause, int? Skip, int? Top);