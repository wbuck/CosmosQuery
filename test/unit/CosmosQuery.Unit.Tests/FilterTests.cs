using AgileObjects.ReadableExpressions;
using CosmosQuery.Unit.Tests.Data;
using CosmosQuery.Unit.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using System.Globalization;
using System.Linq.Expressions;

namespace CosmosQuery.Unit.Tests;

public sealed class FilterTests
{
    private readonly IServiceProvider serviceProvider;

    public FilterTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
            .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

        serviceProvider = services.BuildServiceProvider();
    }

    #region Inequalities
    [Theory]
    [InlineData(null, true)]
    [InlineData("", false)]
    [InlineData("Doritos", false)]
    public void EqualityOperatorWithNull(string productName, bool expected)
    {
        var filter = GetFilter<Product>("ProductName eq null");
        bool result = RunFilter(filter, new Product { ProductName = productName });

        AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == null)");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("Doritos", true)]
    public void EqualityOperator(string productName, bool expected)
    {
        var filter = GetFilter<Product>("ProductName eq 'Doritos'");
        bool result = RunFilter(filter, new Product { ProductName = productName });

        AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == \"Doritos\")");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("Doritos", false)]
    public void NotEqualOperator(string productName, bool expected)
    {
        var filter = GetFilter<Product>("ProductName ne 'Doritos'");
        bool result = RunFilter(filter, new Product { ProductName = productName });

        AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName != \"Doritos\")");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(5.01, true)]
    [InlineData(4.99, false)]
    public void GreaterThanOperator(object unitPrice, bool expected)
    {
        var filter = GetFilter<Product>("UnitPrice gt 5.00m");
        bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

        AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice > Convert({0:0.00}))", 5.0));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(5.0, true)]
    [InlineData(4.99, false)]
    public void GreaterThanEqualOperator(object unitPrice, bool expected)
    {
        var filter = GetFilter<Product>("UnitPrice ge 5.00m");
        bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });
        
        AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice >= Convert({0:0.00}))", 5.0));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(4.99, true)]
    [InlineData(5.01, false)]
    public void LessThanOperator(object unitPrice, bool expected)
    {
        var filter = GetFilter<Product>("UnitPrice lt 5.00m");
        bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

        AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice < Convert({0:0.00}))", 5.0));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(5.0, true)]
    [InlineData(5.01, false)]
    public void LessThanOrEqualOperator(object unitPrice, bool expected)
    {
        var filter = GetFilter<Product>("UnitPrice le 5.00m");
        bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

        AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", 5.0));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NegativeNumbers()
    {
        var filter = GetFilter<Product>("UnitPrice le -5.00m");
        bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(44m) });

        AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", -5.0));
        Assert.False(result);
    }

    [Theory]
    [InlineData("DateTimeOffsetProp eq DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == $it.DateTimeOffsetProp)")]
    [InlineData("DateTimeOffsetProp ne DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp != $it.DateTimeOffsetProp)")]
    [InlineData("DateTimeOffsetProp ge DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp >= $it.DateTimeOffsetProp)")]
    [InlineData("DateTimeOffsetProp le DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp <= $it.DateTimeOffsetProp)")]
    public void DateTimeOffsetInequalities(string clause, string expectedExpression)
    {        
        var filter = GetFilter<DataTypes>(clause);
        AssertFilterStringIsCorrect(filter, expectedExpression);
    }

    [Theory]
    [InlineData("DateTimeProp eq DateTimeProp", "$it => ($it.DateTimeProp == $it.DateTimeProp)")]
    [InlineData("DateTimeProp ne DateTimeProp", "$it => ($it.DateTimeProp != $it.DateTimeProp)")]
    [InlineData("DateTimeProp ge DateTimeProp", "$it => ($it.DateTimeProp >= $it.DateTimeProp)")]
    [InlineData("DateTimeProp le DateTimeProp", "$it => ($it.DateTimeProp <= $it.DateTimeProp)")]
    public void DateInEqualities(string clause, string expectedExpression)
    {
        var filter = GetFilter<DataTypes>(clause);
        AssertFilterStringIsCorrect(filter, expectedExpression);
    }
    #endregion Inequalities

    private static T? ToNullable<T>(object value) where T : struct =>
        value == null ? null : (T?)Convert.ChangeType(value, typeof(T));

    private static void AssertFilterStringIsCorrect(Expression expression, string expected)
    {
        string result = ExpressionStringBuilder.ToString(expression);
        Assert.True
        (
            expected == result, 
            $"Expected expression '{expected}' but the deserializer produced '{result}'"
        );
    }

    private static bool RunFilter<TModel>(Expression<Func<TModel, bool>> filter, TModel instance)
        => filter.Compile().Invoke(instance);

    private static Expression<Func<TElement, bool>> GetSelectNestedFilter<TModel, TElement>(string selectString) where TModel : class
        => GetSelectNestedFilter<TModel, TElement>
           (
                new Dictionary<string, string> { ["$select"] = selectString }
           );

    private static Expression<Func<TElement, bool>> GetSelectNestedFilter<TModel, TElement>(IDictionary<string, string> queryOptions) where TModel : class
    {
        var selectAndExpand = ODataHelpers.GetSelectExpandClause<TModel>(queryOptions);
        var filterOption = selectAndExpand.SelectedItems.OfType<PathSelectItem>().Select(p => p.FilterOption).First();
        return (Expression<Func<TElement, bool>>)filterOption.GetFilterExpression
        (
            typeof(TElement),
            ODataHelpers.GetODataQueryContext<TModel>()
        );
    }

    private Expression<Func<TModel, bool>> GetFilter<TModel>(string filterString) where TModel : class
        => GetFilter<TModel>
        (
            new Dictionary<string, string> { ["$filter"] = filterString }
        );

    private Expression<Func<TModel, bool>> GetFilter<TModel>(IDictionary<string, string> queryOptions, bool useFilterOption = false) where TModel : class
       => GetFilterExpression<TModel>
       (
           GetFilterClause<TModel>(queryOptions, useFilterOption)
    );

    private FilterClause GetFilterClause<TModel>(IDictionary<string, string> queryOptions, bool useFilterOption = false) where TModel : class
        => ODataHelpers.GetFilterClause<TModel>(queryOptions, serviceProvider, useFilterOption);

    private static Expression<Func<TModel, bool>> GetFilterExpression<TModel>(FilterClause filterClause) where TModel : class
        => (Expression<Func<TModel, bool>>)filterClause.GetFilterExpression(typeof(TModel), ODataHelpers.GetODataQueryContext<TModel>());
}