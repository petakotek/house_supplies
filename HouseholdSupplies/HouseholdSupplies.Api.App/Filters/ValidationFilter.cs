using System.Net;
using FluentValidation;

namespace HouseholdSupplies.Api.App.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        T? argumentToValidate = context.GetArgument<T>(0);
        if (argumentToValidate is null)
            return Results.BadRequest();

        IValidator<T>? validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is not null)
        {
            var validationResult = await validator.ValidateAsync(argumentToValidate);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary(),
                    statusCode: validationResult.Errors.Any(error => error.ErrorCode == "ReferenceNotFound")
                        ? (int)HttpStatusCode.BadRequest
                        : (int)HttpStatusCode.UnprocessableEntity);
            }
        }

        return await next.Invoke(context);
    }
}
