using Microsoft.OData.Edm;
using System.Diagnostics.CodeAnalysis;

namespace CosmosQuery.Edm;
internal readonly struct EdmTypeStructure : IEquatable<EdmTypeStructure>
{
    public EdmTypeStructure(IEdmTypeReference edmTypeReference)
    {
        FullName = edmTypeReference.FullName();
        IsNullable = edmTypeReference.IsNullable;
    }

    public EdmTypeStructure(string fullName, bool isNullable)
    {
        FullName = fullName;
        IsNullable = isNullable;
    }

    public string FullName { get; }
    public bool IsNullable { get; }

    public bool Equals(EdmTypeStructure other)
        => this.FullName == other.FullName && this.IsNullable == other.IsNullable;

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is EdmTypeStructure type && Equals(type);

    public override int GetHashCode() => 
        FullName.GetHashCode();
}
