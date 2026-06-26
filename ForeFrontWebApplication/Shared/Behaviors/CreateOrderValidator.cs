using FluentValidation;
using ForeFrontWebApplication.Commands;

namespace ForeFrontWebApplication.Shared.Behaviors;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.KundId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Produkter)
            .NotEmpty().WithMessage("Minst en produkt krävs.")
            .Must(p => p.Count <= 50).WithMessage("Max 50 produkter per order.");

        RuleForEach(x => x.Produkter).ChildRules(item =>
        {
            item.RuleFor(p => p.ProduktId)
                .NotEmpty()
                .MaximumLength(100);

            item.RuleFor(p => p.Antal)
                .InclusiveBetween(1, 10_000)
                .WithMessage("Antal måste vara mellan 1 och 10 000.");
        });
    }
}
