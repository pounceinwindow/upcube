namespace UpperCube.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency = "USD")
    {
        return new Money(0, currency);
    }
}