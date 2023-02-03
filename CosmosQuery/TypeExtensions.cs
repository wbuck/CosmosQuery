using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Reflection;

namespace CosmosQuery
{
    internal static class TypeExtensions
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy |
            BindingFlags.IgnoreCase;

        public static MemberInfo[] GetValueOrListOfValueTypeMembers(this Type parentType)
        {
            if (parentType.IsList())
                return Array.Empty<MemberInfo>();

            return parentType.GetMemberInfos().Where
            (
                info =>
                   (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property) &&
                   (info.GetMemberType().IsLiteralType() || info.IsListOfValueTypes())
            ).ToArray();
        }

        public static MemberInfo[] GetFieldsAndProperties(this Type parentType) =>
            parentType.GetMemberInfos()
                .Where(info => info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
                .ToArray();

        private static MemberInfo[] GetMemberInfos(this Type parentType)
            => parentType.GetMembers(InstanceFlags);

        public static MemberInfo[] GetSelectedMembers(this Type parentType, List<string> selects)
        {
            if (selects == null || !selects.Any())
                return parentType.GetValueTypeMembers();

            return selects.Select(select => parentType.GetMemberInfo(select)).ToArray();
        }

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

        public static Type GetClrType(string fullName, bool isNullable, IDictionary<EdmTypeStructure, Type> typesCache)
            => GetClrType(new EdmTypeStructure(fullName, isNullable), typesCache);

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IDictionary<EdmTypeStructure, Type> typesCache)
            => edmTypeReference == null
                ? typeof(object)
                : GetClrType(new EdmTypeStructure(edmTypeReference), typesCache);

        private static Type GetClrType(EdmTypeStructure edmTypeStructure, IDictionary<EdmTypeStructure, Type> typesCache)
        {
            if (typesCache.TryGetValue(edmTypeStructure, out Type type))
                return type;

            type = LoadedTypes.SingleOrDefault
            (
                item => edmTypeStructure.FullName == item.FullName
            );

            if (type != null)
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
            if (edmTypeReference == null)
                return typeof(object);

            return edmModel.GetTypeMapper().GetClrType(edmModel, edmTypeReference, _AssemblyResolver);
        }

        private static IAssemblyResolver _assemblyResolver;
        private static IAssemblyResolver _AssemblyResolver
        {
            get
            {
                _assemblyResolver ??= new AssemblyResolver();

                return _assemblyResolver;
            }
        }

        private static IList<Type> _loadedTypes = null;
        public static IList<Type> LoadedTypes
        {
            get
            {
                _loadedTypes ??= GetAllTypes(AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList());

                return _loadedTypes;
            }
        }

        public static IList<Type> GetAllTypes(List<Assembly> assemblies)
        {
            return DoLoad(new List<Type>());

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
                            ex.Types.Where(type => type != null && type.IsPublic && type.IsVisible)
                        );
                    }
                });

                return allTypes;
            }
        }

        public static Dictionary<EdmTypeStructure, Type> GetEdmToClrTypeMappings() => Constants.EdmToClrTypeMappings;

        public static bool IsListOfValueTypes(this MemberInfo memberInfo) =>
            memberInfo.GetMemberType().IsListOfValueTypes();

        public static bool IsListOfValueTypes(this Type memberType) =>
            memberType.IsList() && memberType.GetUnderlyingElementType().IsLiteralType();

        private class AssemblyResolver : IAssemblyResolver
        {
            private List<Assembly> _assemblides;
            public IEnumerable<Assembly> Assemblies
            {
                get
                {
                    if (_assemblides == null)
                        _assemblides = AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList();

                    return _assemblides;
                }
            }
        }

    }
}
