using FluentValidation;

namespace GroceryChef.Api.DTOs.Ingredients;

public sealed class CreateIngredientDtoValidator : AbstractValidator<CreateIngredientDto>
{
    public CreateIngredientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Ingredient name must be provided and cannot exceed 500 characters.");

        RuleFor(x => x.ShelfLifeOfDate)
            .GreaterThan(-1)
            .WithMessage("Shelf life of date must be greater than 0 days.");

        RuleFor(x => x.IsAllergy)
            .NotNull()
            .WithMessage("Allergy must be specified.");
    }
}
