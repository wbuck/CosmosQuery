using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LogicBuilder.Expressions.Utils;

namespace CosmosQuery.Cache;
internal static class TypeCache
{
    private static readonly ConcurrentDictionary<Type, MemberCache> Cache = new();

    public static IMemberCache GetOrAdd(Type parentType) =>
        Cache.GetOrAdd(parentType, type => new(type));

    public static IMemberCache GetOrAdd<T>() =>
        GetOrAdd(typeof(T));

    public static bool Remove<T>() =>
        Remove(typeof(T));

    public static bool Remove(Type parentType) =>
        Cache.Remove(parentType, out var _);

    public static void Clear() =>
        Cache.Clear();

    private sealed class MemberCache : IMemberCache
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy |
            BindingFlags.IgnoreCase;

        private readonly ImmutableDictionary<string, MemberInfo> cache;

        public MemberCache(Type parentType)
        {
            parentType = parentType ?? throw new ArgumentNullException(nameof(parentType));

            MemberInfo[] members = GetFieldsAndProperties(parentType);
            this.cache = members.ToImmutableDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
        }

        public int Count => cache.Count;

        public bool ContainsKey(string key) =>
            this.cache.ContainsKey(key);

        public IEnumerable<string> Keys =>
            this.cache.Keys;

        public IEnumerable<MemberInfo> Values =>
            this.cache.Values;

        public IEnumerator<KeyValuePair<string, MemberInfo>> GetEnumerator() =>
            this.cache.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out MemberInfo member) =>
            this.cache.TryGetValue(key, out member);

        private static MemberInfo[] GetFieldsAndProperties(Type parentType) =>
            GetMemberInfos(parentType)
                .Where(info => info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
                .ToArray();

        private static MemberInfo[] GetMemberInfos(Type parentType)
            => parentType.GetMembers(InstanceFlags);
    }
}
