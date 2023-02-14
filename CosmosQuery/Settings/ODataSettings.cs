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
using Microsoft.AspNetCore.OData.Query;


namespace CosmosQuery;

/// <summary>
/// Settings for configuring OData options on the server
/// </summary>
public sealed record ODataSettings
{
    /// <summary>
    /// Gets or sets a value indicating how null propagation should
    /// be handled during query composition.
    /// </summary>
    /// <value>
    /// The default is <see cref="F:Microsoft.AspNet.OData.Query.HandleNullPropagationOption.Default" />.
    /// </value>
    public HandleNullPropagationOption HandleNullPropagation { get; init; }
        = HandleNullPropagationOption.Default;

    /// <summary>
    /// Gets or sets the maximum number of query results to return.
    /// </summary>
    /// <value>
    /// The maximum number of query results to return, or null if there is no limit. Default is null.
    /// </value>
    public int? PageSize { get; init; }

    /// <summary>
    /// Gets of sets the <see cref="TimeZoneInfo"/>.
    /// </summary>
    /// <value>
    /// Default is null.
    /// </value>
    public TimeZoneInfo? TimeZone { get; init; }
}
