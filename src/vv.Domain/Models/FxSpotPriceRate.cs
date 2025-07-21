using System;
using vv.Domain.Models.Exceptions;

namespace vv.Domain.Models
{
    /// <summary>
    /// Domain model for an FX Spot Price Rate
    /// </summary>
    public class FxSpotPriceRate
    {
        /// <summary>
        /// Base currency (e.g., EUR in EUR/USD)
        /// </summary>
        public string BaseCurrency { get; set; } = string.Empty;

        /// <summary>
        /// Quote currency (e.g., USD in EUR/USD)
        /// </summary>
        public string QuoteCurrency { get; set; } = string.Empty;

        /// <summary>
        /// Exchange rate value
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Date of the rate
        /// </summary>
        public DateOnly AsOfDate { get; set; }
        
        /// <summary>
        /// </summary>
        public string AssetId { get; set; } = string.Empty;
        
        /// <summary>
        /// </summary>
        public decimal BidPrice { get; set; }
        
        /// <summary>
        /// </summary>
        public decimal AskPrice { get; set; }

        /// <summary>
        /// Creates a domain object from a data entity
        /// </summary>
        public static FxSpotPriceRate? FromEntity(FxSpotPriceData? entity)
        {
            if (entity == null)
                return null;

            // Parse the asset ID to get base and quote currencies (e.g., "eurusd")
            string assetId = entity.AssetId.ToLowerInvariant();
            if (assetId.Length != 6)
                throw new ArgumentException($"Invalid asset ID format: {assetId}. Expected format is 6 characters (e.g., 'eurusd')");

            string baseCurrency = assetId.Substring(0, 3).ToUpperInvariant();
            string quoteCurrency = assetId.Substring(3, 3).ToUpperInvariant();

            return new FxSpotPriceRate
            {
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency,
                Rate = entity.Rate,
                AsOfDate = entity.AsOfDate,
                AssetId = entity.AssetId ?? string.Empty,
                BidPrice = entity.Rate * 0.999m, // Slightly lower than mid rate
                AskPrice = entity.Rate * 1.001m  // Slightly higher than mid rate
            };
        }

        public decimal CalculateMidPrice() => (BidPrice + AskPrice) / 2;

        public decimal CalculateSpread() => AskPrice - BidPrice;

        public bool IsCrossPair() =>
            !AssetId.StartsWith("usd", StringComparison.OrdinalIgnoreCase) &&
            !AssetId.EndsWith("usd", StringComparison.OrdinalIgnoreCase);

        // Validation logic
        public void Validate()
        {
            if (BidPrice <= 0 || AskPrice <= 0)
                throw new InvalidMarketDataException("Prices must be positive");

            if (BidPrice > AskPrice)
                throw new InvalidMarketDataException("Bid price cannot be higher than ask price");
        }
    }
}
