using FluentValidation;
using HouseholdSupplies.Common.Models.User;

namespace HouseholdSupplies.Api.App.Validators;

public class UserListModelValidator : AbstractValidator<UserListModel>
{
    public UserListModelValidator()
    {
        RuleFor(user => user.Name).NotEmpty();
        RuleFor(user => user.ImageUrl).NotEmpty();
        RuleFor(user => user.Role).IsInEnum();
    }
}
