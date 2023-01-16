using CosmosQuery.Edm;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Reflection;

namespace CosmosQuery.Extensions;
internal static class TypeExt
{    
    public static MemberInfo[] GetSelectedMembers(this Type parentType, List<string> selects) =>
        selects is null || !selects.Any()
            ? parentType.GetValueTypeMembers()
            : selects.Select(select => parentType.GetMemberInfo(select)).ToArray();

    private static MemberInfo[] GetValueTypeMembers(this Type parentType)
    {
        if (parentType.IsList())
            return Array.Empty<MemberInfo>();

        return parentType.GetMemberInfos().Where
        (
            info => (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
            && info.GetMemberType().IsLiteralType()
        ).ToArray();
    }

    private static MemberInfo[] GetMemberInfos(this Type parentType)
        => parentType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

    public static Type GetClrType(string fullName, bool isNullable, IDictionary<EdmTypeStructure, Type> typesCache)
        => GetClrType(new EdmTypeStructure(fullName, isNullable), typesCache);

    public static Type GetClrType(IEdmTypeReference edmTypeReference, IDictionary<EdmTypeStructure, Type> typesCache)
        => edmTypeReference is null
            ? typeof(object)
            : GetClrType(new EdmTypeStructure(edmTypeReference), typesCache);

    private static Type GetClrType(EdmTypeStructure edmTypeStructure, IDictionary<EdmTypeStructure, Type> typesCache)
    {
        if (typesCache.TryGetValue(edmTypeStructure, out Type? type))
            return type;

        type = LoadedTypes.SingleOrDefault
        (
            item => edmTypeStructure.FullName == item.FullName
        );

        if (type is not null)
        {
            if (type.IsValueType && !type.IsNullableType() && edmTypeStructure.IsNullable)
            {
                type = type.ToNullable();
                typesCache.Add(edmTypeStructure, type);
            }

            return type;
        }

        throw new ArgumentException($"Cannot find CLT type for EDM type {edmTypeStructure.FullName}");
    }

    public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel, IDictionary<EdmTypeStructure, Type> typesCache)
    {
        if (edmTypeReference is null)
            return typeof(object);

        return edmModel.GetTypeMapper()
            .GetClrType(edmModel, edmTypeReference, assemblyResolver.Value);
    }    

    public static IList<Type> LoadedTypes =>
        loadedTypes.Value;

    public static IList<Type> GetAllTypes(List<Assembly> assemblies)
    {
        return DoLoad(new());

        List<Type> DoLoad(List<Type> allTypes)
        {
            assemblies.ForEach(assembly =>
            {
                try
                {
                    allTypes.AddRange(assembly.GetTypes().Where(type => type.IsPublic && type.IsVisible));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    allTypes.AddRange
                    (
                        ex.Types
                            .Where(type => type is not null && type.IsPublic && type.IsVisible)
                            .Select(type => type!)
                    );
                }
            });

            return allTypes;
        }
    }

    public static Dictionary<EdmTypeStructure, Type> GetEdmToClrTypeMappings() => Constants.EdmToClrTypeMappings;

    public static bool IsListOfLiteralTypes(this MemberInfo memberInfo)
    {
        var memberType = memberInfo.GetMemberType();
        return memberType.IsListOfLiteralTypes();
    }

    public static bool IsListOfLiteralTypes(this Type type) =>
        type.IsList() && type.GetUnderlyingElementType().IsLiteralType();

    private static readonly Lazy<IAssemblyResolver> assemblyResolver
        = new(() => new AssemblyResolver());

    private static readonly Lazy<IList<Type>> loadedTypes = new(() =>
        GetAllTypes(AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList()));

    private sealed class AssemblyResolver : IAssemblyResolver
    {
        private readonly Lazy<List<Assembly>> assemblies = new(() => 
            AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList());

        public IEnumerable<Assembly> Assemblies => 
            this.assemblies.Value;
    }
}
