namespace vv.Domain.Models;

/// <summary>
/// Provides predefined constants representing various data types for market data classification.
/// </summary>
public static class DataTypes
{
    public const string PriceSpot = "price.spot";
    public const string PriceForward = "price.forward";
    public const string Vol = "vol";
    public const string VolSurface = "vol.surface";
    public const string OrderBook = "orderbook";
    public const string Perpetual = "perpetual";
}
