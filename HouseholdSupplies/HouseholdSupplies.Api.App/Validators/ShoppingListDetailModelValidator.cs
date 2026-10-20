using FluentValidation;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.ShoppingList;

namespace HouseholdSupplies.Api.App.Validators;

public class ShoppingListDetailModelValidator : AbstractValidator<ShoppingListDetailModel>
{
    public ShoppingListDetailModelValidator(IUserRepository userRepository)
    {
        RuleFor(shoppingList => shoppingList.Name).NotEmpty();
        RuleFor(shoppingList => shoppingList.Shop).NotEmpty();
        RuleFor(shoppingList => shoppingList.Author).NotNull();
        RuleFor(shoppingList => shoppingList.Author!.Id)
            .Must(userRepository.Exists)
            .When(shoppingList => shoppingList.Author is not null)
            .WithMessage("Author with id '{PropertyValue}' does not exist.");
    }
}
