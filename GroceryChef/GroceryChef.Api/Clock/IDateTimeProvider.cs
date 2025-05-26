namespace GroceryChef.Api.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
