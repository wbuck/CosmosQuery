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
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;

namespace CosmosQuery.Extensions
{
    internal static class ODataQueryOptionsExtensions
    {
        /// <summary>
        /// Adds the expand options to the result.
        /// </summary>
        /// <param name="options"></param>
        public static void AddExpandOptionsResult(this ODataQueryOptions options)
        {
            if (options.SelectExpand == null)
                return;

            options.Request.ODataFeature().SelectExpandClause = options.SelectExpand.SelectExpandClause;
        }

        /// <summary>
        /// Adds the count options to the result.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="longCount"></param>
        public static void AddCountOptionsResult(this ODataQueryOptions options, long longCount)
        {
            if (options.Count?.Value != true)
                return;

            options.Request.ODataFeature().TotalCount = longCount;
        }

        /// <summary>
        /// Adds the next link options to the result.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="pageSize"></param>
        public static void AddNextLinkOptionsResult(this ODataQueryOptions options, int pageSize)
        {
            if (options.Request == null)
                return;

            options.Request.ODataFeature().NextLink = options.Request.GetNextPageLink(pageSize, null, null);
        }
    }
}
