using FluentValidation;
using HouseholdSupplies.Api.DAL.Repositories.Interfaces;
using HouseholdSupplies.Common.Models.Place;

namespace HouseholdSupplies.Api.App.Validators;

public class PlaceDetailModelValidator : AbstractValidator<PlaceDetailModel>
{
    public PlaceDetailModelValidator(IUserRepository userRepository)
    {
        RuleFor(place => place.Name).NotEmpty();
        RuleFor(place => place.Author).NotNull();
        RuleFor(place => place.Author!.Id)
            .Must(userRepository.Exists)
            .When(place => place.Author is not null)
            .WithMessage("Author with id '{PropertyValue}' does not exist.");
    }
}
