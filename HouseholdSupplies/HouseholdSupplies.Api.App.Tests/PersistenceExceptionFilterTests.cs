using System.Reflection;
using HouseholdSupplies.Api.App.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HouseholdSupplies.Api.App.Tests;

public sealed class PersistenceExceptionFilterTests
{
    [Theory]
    [InlineData(547)] // Foreign key or check constraint.
    [InlineData(2601)] // Unique index.
    [InlineData(2627)] // Primary key or unique constraint.
    [InlineData(515)] // Required column.
    public async Task Constraint_Violations_Return_Conflict(int errorNumber)
    {
        // Arrange
        var filter = new PersistenceExceptionFilter();
        var exception = new DbUpdateException("Save failed", CreateSqlException(errorNumber));

        // Act
        var result = await filter.InvokeAsync(EndpointFilterInvocationContext.Create(new DefaultHttpContext()),
            _ => ValueTask.FromException<object?>(exception));

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(problem.ProblemDetails.Title));
        Assert.False(string.IsNullOrWhiteSpace(problem.ProblemDetails.Detail));
        Assert.DoesNotContain("Save failed", problem.ProblemDetails.Detail);
    }

    [Theory]
    [InlineData(-2)] // Timeout.
    [InlineData(18456)] // Login failure.
    public async Task Other_Database_Errors_Propagate(int errorNumber)
    {
        // Arrange
        var filter = new PersistenceExceptionFilter();
        var exception = new DbUpdateException("Save failed", CreateSqlException(errorNumber));

        // Act
        var actual = await Record.ExceptionAsync(async () =>
            await filter.InvokeAsync(EndpointFilterInvocationContext.Create(new DefaultHttpContext()),
                _ => ValueTask.FromException<object?>(exception)));

        // Assert
        Assert.Same(exception, actual);
    }

    [Fact]
    public async Task Successful_Endpoint_Result_Is_Preserved()
    {
        // Arrange
        var filter = new PersistenceExceptionFilter();
        var expected = Results.Ok("Saved");

        // Act
        var result = await filter.InvokeAsync(EndpointFilterInvocationContext.Create(new DefaultHttpContext()),
            _ => ValueTask.FromResult<object?>(expected));

        // Assert
        Assert.Same(expected, result);
    }

    private static SqlException CreateSqlException(int number)
    {
        // SqlClient exposes no public factory for server errors. Construct a real provider
        // exception so these focused tests do not require a running SQL Server.
        var errorConstructor = typeof(SqlError).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string),
                typeof(int), typeof(int), typeof(Exception)], null)!;
        var error = (SqlError)errorConstructor.Invoke(
            [number, (byte)1, (byte)16, "test-server", "Test database error", "", 1, 0, null]);
        var errors = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), nonPublic: true)!;
        typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(errors, [error]);
        return (SqlException)typeof(SqlException).GetMethod("CreateException",
            BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(SqlErrorCollection), typeof(string)], null)!
            .Invoke(null, [errors, "16.0"])!;
    }
}
