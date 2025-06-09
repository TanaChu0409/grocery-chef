using FluentValidation;

namespace GroceryChef.Api.DTOs.Carts;

public sealed class CreateCartDtoValidator : AbstractValidator<CreateCartDto>
{
    public CreateCartDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Cart name must be provided and cannot exceed 500 characters.");
    }
}
