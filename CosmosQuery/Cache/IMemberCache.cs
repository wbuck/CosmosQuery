using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CosmosQuery.Cache;

internal interface IMemberCache : IEnumerable, IEnumerable<KeyValuePair<string, MemberInfo>>
{
    int Count { get; }
    IEnumerable<string> Keys { get; }
    IEnumerable<MemberInfo> Values { get; }
    bool ContainsKey(string key);
    bool TryGetValue(string key, [MaybeNullWhen(false)] out MemberInfo member);
}
