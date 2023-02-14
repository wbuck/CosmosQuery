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
using CosmosQuery.Settings;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;

namespace CosmosQuery.Extensions
{
    internal static class ODataQueryContextExtentions
    {
        public static OrderBySetting? FindSortableProperties(this ODataQueryContext context, Type type)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            IEdmSchemaElement? schemaElement = GetSchemaElement();

            return schemaElement is not null
                ? FindProperties(schemaElement)
                : null;

            IEdmSchemaElement? GetSchemaElement() =>
                context.Model.SchemaElements
                    .FirstOrDefault(e => (e is IEdmEntityType || e is IEdmComplexType) && e.Name == type.Name);

            static OrderBySetting? FindProperties(IEdmSchemaElement schemaElement)
            {
                var propertyNames = schemaElement switch
                {
                    IEdmEntityType entityType when entityType.ContainsKey() => entityType.Key().Select(p => p.Name),
                    IEdmStructuredType structuredType => structuredType.GetSortableProperties().Take(1),
                    _ => throw new NotSupportedException("The EDM element type is not supported.")
                };

                var orderBySettings = new OrderBySetting();
                propertyNames.Aggregate(orderBySettings, (settings, name) =>
                {
                    if (settings.Name is null)
                    {
                        settings.Name = name;
                        return settings;
                    }
                    settings.ThenBy = new() { Name = name };
                    return settings.ThenBy;
                });
                return orderBySettings.Name is null ? null : orderBySettings;
            }
        }

        private static bool ContainsKey(this IEdmEntityType entityType)
            => entityType.Key().Any();

        private static IEnumerable<string> GetSortableProperties(this IEdmStructuredType structuredType)
            => structuredType.StructuralProperties()
                .Where(p => p.Type.IsPrimitive() && !p.Type.IsStream())
                .Select(p => p.Name)
                .OrderBy(n => n);
    }
}
