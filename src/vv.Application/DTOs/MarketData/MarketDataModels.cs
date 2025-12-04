using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.MarketData
{
    public class CryptoSpotPriceDto
    {
        public required string Exchange { get; set; }
        public required string Symbol { get; set; }
        public required string BaseAsset { get; set; }
        public required string QuoteAsset { get; set; }
        public decimal LastPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
        public decimal Volume24H { get; set; }
        public decimal PriceChangePercent24H { get; set; } //
        public DateTime Timestamp { get; set; }
    }

    public class CryptoMarketSummaryDto
    {
        public required string BaseAsset { get; set; }
        public required string QuoteAsset { get; set; }
        public List<ExchangePriceDto> Prices { get; set; } = new();
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public decimal MedianPrice { get; set; }
        public decimal AverageVolume { get; set; }
        public decimal ArbitrageOpportunity { get; set; }
    }

    public class ExchangePriceDto
    {
        public required string Exchange { get; set; }
        public decimal Price { get; set; }
        public decimal Volume24H { get; set; }
    }

    public class HistoricalDataPointDto
    {
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
    }

    public class MarketDepthDto
    {
        public required string Exchange { get; set; }
        public required string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public List<PriceVolumeDto> Bids { get; set; } = new();
        public List<PriceVolumeDto> Asks { get; set; } = new();
        public decimal TotalBidVolume { get; set; }
        public decimal TotalAskVolume { get; set; }
        public decimal BidAskSpread { get; set; }
        public decimal BidAskSpreadPercentage { get; set; }
    }

    public class PriceVolumeDto
    {
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }

    public class MarketTickerDto
    {
        public required string Exchange { get; set; }
        public required string Symbol { get; set; }
        public required string BaseAsset { get; set; }
        public required string QuoteAsset { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal VolumeBaseAsset { get; set; }
        public decimal VolumeQuoteAsset { get; set; }
        public DateTime Timestamp { get; set; }
    }
}