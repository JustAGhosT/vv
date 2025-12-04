using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.LiquidityPools
{
    public class LiquidityPoolDto
    {
        public required string PoolId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string RiskCategory { get; set; } // Conservative, Moderate, Aggressive
        public decimal TotalValueUsd { get; set; }
        public Dictionary<string, decimal> AssetAllocations { get; set; } = new(); // Asset → Percentage
        public List<VaultStrategyDto> ActiveStrategies { get; set; } = new();
        public required PoolPerformanceMetricsDto Performance { get; set; }
        public required LiquidityDistributionDto Distribution { get; set; }
        public DateTime LastRebalanced { get; set; }
        public List<string> SupportedAssets { get; set; } = new();
        public decimal ManagementFeeRate { get; set; } // Annual fee %
        public decimal PerformanceFeeRate { get; set; } // % of profits
        public decimal CurrentYield { get; set; } // Annualized yield %
        public int ClientCount { get; set; }
        public decimal MinimumInvestment { get; set; }
        public bool AcceptingNewInvestors { get; set; }
    }

    public class PoolPerformanceMetricsDto
    {
        public decimal ReturnLast24Hours { get; set; }
        public decimal ReturnLast7Days { get; set; }
        public decimal ReturnLast30Days { get; set; }
        public decimal ReturnYTD { get; set; }
        public decimal Volatility30Day { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal SortinoRatio { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal AverageUtilizationRate { get; set; } // How much of the pool is actively deployed
        public Dictionary<string, decimal> ReturnByAsset { get; set; } = new(); // Asset → Return %
        public List<PerformanceDataPointDto> DailyPerformance { get; set; } = new();
        public decimal ImpermanentLoss { get; set; }
    }

    public class PerformanceDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal ValueUsd { get; set; }
        public decimal DailyChangePercent { get; set; }
        public decimal YieldToDate { get; set; }
    }

    public class VaultStrategyDto
    {
        public required string StrategyId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Category { get; set; } // Market Making, Arbitrage, Yield Farming, etc.
        public Dictionary<string, decimal> AllocationPercentages { get; set; } = new(); // Asset → %
        public List<string> TargetExchanges { get; set; } = new();
        public Dictionary<string, decimal> StrategyParameters { get; set; } = new(); // Parameter name → value
        public decimal CurrentProfitLoss { get; set; }
        public decimal HistoricalReturn { get; set; }
        public decimal RiskScore { get; set; } // 1-10 scale
        public DateTime ImplementedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class LiquidityDistributionDto
    {
        public Dictionary<string, LiquidityVenueDto> DistributionByVenue { get; set; } = new();
        public Dictionary<string, decimal> DistributionByAsset { get; set; } = new();
        public decimal TotalLiquidityUsd { get; set; }
        public decimal AvailableLiquidityUsd { get; set; }
        public decimal CommittedLiquidityUsd { get; set; }
        public decimal ReserveLiquidityUsd { get; set; } // Emergency reserves
    }

    public class LiquidityVenueDto
    {
        public required string VenueName { get; set; } // Exchange name or DeFi protocol
        public required string VenueType { get; set; } // CEX, DEX, AMM, etc.
        public decimal AllocatedAmount { get; set; }
        public decimal CurrentAmount { get; set; } // May differ due to P&L
        public decimal UtilizationRate { get; set; }
        public Dictionary<string, decimal> AssetBreakdown { get; set; } = new();
        public decimal CurrentAPY { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal GasExposure { get; set; } // For DeFi venues
        public bool IsActive { get; set; }
    }

    public class PoolRebalanceDto
    {
        public required string RebalanceId { get; set; }
        public required string PoolId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? ExecutedTime { get; set; }
        public required string Status { get; set; } // Scheduled, InProgress, Completed, Failed
        public List<AssetAllocationChangeDto> AllocationChanges { get; set; } = new();
        public List<string> AffectedVenues { get; set; } = new();
        public decimal EstimatedFeesCost { get; set; }
        public decimal ActualFeesCost { get; set; }
        public required string ExecutedBy { get; set; }
        public required string RebalanceReason { get; set; }
        public Dictionary<string, decimal> PerformanceImpact { get; set; } = new();
    }

    public class AssetAllocationChangeDto
    {
        public required string Asset { get; set; }
        public decimal PreviousPercentage { get; set; }
        public decimal NewPercentage { get; set; }
        public decimal AbsoluteChange { get; set; }
        public decimal AmountUsd { get; set; }
        public required string Direction { get; set; } // Increase, Decrease
    }
}