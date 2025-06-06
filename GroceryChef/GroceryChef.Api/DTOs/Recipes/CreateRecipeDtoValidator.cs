using FluentValidation;

namespace GroceryChef.Api.DTOs.Recipes;

public sealed class CreateRecipeDtoValidator : AbstractValidator<CreateRecipeDto>
{
    public CreateRecipeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Recipe name must be provided and cannot exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(3000)
            .WithMessage("Recipe content must be provided and cannot exceed 3000 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(100)
            .WithMessage("Recipe description cannot exceed 100 characters.");
    }
}
