using ForeFrontWebApplication.Commands;
using FluentValidation;
using ForeFrontWebApplication.DTOs.Order;
using ForeFrontWebApplication.Shared.Behaviors;
using Xunit;

namespace ForeFrontWebApplication.Tests.Validators;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _sut = new();

    private static OrderItemRequest ValidItem(string produktId = "p1", int antal = 1) =>
        new() { ProduktId = produktId, Antal = antal };

    private static OrderItemRequest InvalidItem(string produktId = "", int antal = 0) =>
        new() { ProduktId = produktId, Antal = antal };

    private static CreateOrderCommand ValidCommand() =>
        new("customer-1", [ValidItem()]);

    [Fact]
    public void ShouldFailWhenOneItemIsInvalid()
    {
        var cmd = new CreateOrderCommand("customer-1", [ValidItem(), InvalidItem()]);
        var result = _sut.Validate(cmd);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ShouldFailWhenProdukterIsEmpty()
    {
        var cmd = new CreateOrderCommand(Guid.NewGuid().ToString(), []);

        var result = _sut.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Produkter" &&
            e.ErrorMessage == "Minst en produkt krävs.");
    }

    [Fact]
    public void ShouldPassWhenCommandIsValid()
    {
        var result = _sut.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ShouldFailWhenKundIdIsEmpty()
    {
        var cmd = new CreateOrderCommand("", [ValidItem()]);

        var result = _sut.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "KundId");
    }


    [Fact]
    public void ShouldFailWhenProdukterExceeds50Items()
    {
        var items = Enumerable.Range(1, 51)
            .Select(i => ValidItem($"p{i}"))
            .ToList();
        var cmd = new CreateOrderCommand("customer-1", items);

        var result = _sut.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "Produkter" &&
            e.ErrorMessage == "Max 50 produkter per order.");
    }

    [Fact]
    public void ShouldPassWhenProdukterHasExactly50Items()
    {
        var items = Enumerable.Range(1, 50)
            .Select(i => ValidItem($"p{i}"))
            .ToList();
        var cmd = new CreateOrderCommand("customer-1", items);

        var result = _sut.Validate(cmd);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ShouldFailWhenProduktIdIsEmpty()
    {
        var cmd = new CreateOrderCommand("customer-1", [ValidItem(produktId: "")]);

        var result = _sut.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("ProduktId"));
    }

    [Fact]
    public void ShouldFailWhenAntalIsBelowMinimum()
    {
        var cmd = new CreateOrderCommand("customer-1", [ValidItem(antal: 0)]);

        var result = _sut.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Contains("Antal") &&
            e.ErrorMessage == "Antal måste vara mellan 1 och 10 000.");
    }

    [Fact]
    public void ShouldFailWhenAntalExceedsMaximum()
    {
        var cmd = new CreateOrderCommand("customer-1", [ValidItem(antal: 10_001)]);

        var result = _sut.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName.Contains("Antal") &&
            e.ErrorMessage == "Antal måste vara mellan 1 och 10 000.");
    }
}
