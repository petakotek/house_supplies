using FluentValidation;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.Item;

namespace HouseholdSupplies.Api.App.Validators;

public class ItemDetailModelValidator : AbstractValidator<ItemDetailModel>
{
    public ItemDetailModelValidator(IUserRepository users, IPlaceRepository places)
    {
        RuleFor(item => item.Name).NotEmpty();
        RuleFor(item => item.ImageUrl).NotEmpty();
        RuleFor(item => item.Amount).Must(amount => double.IsFinite(amount) && amount >= 0);
        RuleFor(item => item.Category).IsInEnum();
        RuleFor(item => item.Author).NotNull();
        RuleFor(item => item.Author!.Id).Must(users.Exists).WithErrorCode("ReferenceNotFound")
            .When(item => item.Author is not null)
            .WithMessage("Author with id '{PropertyValue}' does not exist.");
        RuleFor(item => item.Place).NotNull();
        RuleFor(item => item.Place!.Id).Must(places.Exists).WithErrorCode("ReferenceNotFound")
            .When(item => item.Place is not null)
            .WithMessage("Place with id '{PropertyValue}' does not exist.");
    }
}
