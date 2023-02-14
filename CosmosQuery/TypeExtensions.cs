/*
MIT License

Copyright (c) 2019 AutoMapper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
using CosmosQuery.Cache;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Reflection;

namespace CosmosQuery
{
    internal static class TypeExtensions
    {
        private readonly static Lazy<IList<Type>> loadedType = new(
            () => GetAllTypes(AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList()));

        public static MemberInfo[] GetValueOrListOfValueTypeMembers(this Type parentType)
        {
            if (parentType.IsList())
                return Array.Empty<MemberInfo>();

            IMemberCache cache = TypeCache.GetOrAdd(parentType);
            return cache.Values.Where
            (
                info =>
                   (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property) &&
                   (info.GetMemberType().IsLiteralType() || info.IsListOfValueTypes())
            ).ToArray();
        }        

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

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
        {
            if (edmTypeReference is null)
                return typeof(object);

            return edmModel.GetTypeMapper().GetClrType(edmModel, edmTypeReference, AssemblyResolver);
        }

        private static IAssemblyResolver AssemblyResolver { get; } 
            = new Resolver();

        public static IList<Type> LoadedTypes { get; } 
            = loadedType.Value;

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
                            ex.Types.Where(type => type is not null && type.IsPublic && type.IsVisible)!
                        );
                    }
                });

                return allTypes;
            }
        }

        public static Dictionary<EdmTypeStructure, Type> GetEdmToClrTypeMappings() 
            => Constants.EdmToClrTypeMappings;

        public static bool IsListOfValueTypes(this MemberInfo memberInfo) =>
            memberInfo.GetMemberType().IsListOfValueTypes();

        public static bool IsListOfValueTypes(this Type memberType) =>
            memberType.IsList() && memberType.GetUnderlyingElementType().IsLiteralType();

        private sealed class Resolver : IAssemblyResolver
        {
            private readonly Lazy<List<Assembly>> assemblies = new(() 
                => AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList());

            public IEnumerable<Assembly> Assemblies 
                => this.assemblies.Value;
        }
    }
}
