using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.Treasury
{
    public class TreasuryIntegrationDto
    {
        public required string IntegrationId { get; set; }
        public required string CompanyId { get; set; }
        public required string IntegrationType { get; set; } // API, Webhook, File Export
        public required string Status { get; set; }
        public DateTime LastSyncTime { get; set; }
        public List<string> SyncedSystems { get; set; } = new(); // ERP, Accounting Software, etc.
        public Dictionary<string, string> ConfigurationParameters { get; set; } = new();
        public bool AutomaticReporting { get; set; }
        public List<string> ReportRecipients { get; set; } = new();
        public required string ApiKey { get; set; } // Masked for security
    }

    public class TreasuryReportDto
    {
        public required string ReportId { get; set; }
        public required string CompanyId { get; set; }
        public required string ReportType { get; set; } // Monthly Statement, Tax Report, etc.
        public DateTime ReportDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public required string Status { get; set; } // Generated, Delivered, Viewed
        public required string FileUrl { get; set; }
        public List<string> Recipients { get; set; } = new();
        public Dictionary<string, decimal> KeyMetrics { get; set; } = new();
        public List<string> Footnotes { get; set; } = new();
    }

    public class TaxReportDto
    {
        public required string ReportId { get; set; }
        public required string ClientId { get; set; }
        public int TaxYear { get; set; }
        public required string JurisdictionCode { get; set; }
        public required string Status { get; set; } // Draft, Final, Amended
        public DateTime GeneratedAt { get; set; }
        public List<TaxableEventDto> TaxableEvents { get; set; } = new();
        public Dictionary<string, decimal> TotalsByCategory { get; set; } = new();
        public decimal TotalTaxableGains { get; set; }
        public decimal TotalTaxableLosses { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal EstimatedTaxLiability { get; set; }
        public required string FilingInstructions { get; set; }
        public List<string> SupportingDocuments { get; set; } = new();
    }

    public class TaxableEventDto
    {
        public required string EventId { get; set; }
        public required string Type { get; set; } // Yield, Trade, Fee, Interest
        public DateTime Date { get; set; }
        public required string AssetAcquired { get; set; }
        public decimal AmountAcquired { get; set; }
        public decimal ValueAtAcquisitionUsd { get; set; }
        public required string AssetDisposed { get; set; }
        public decimal AmountDisposed { get; set; }
        public decimal ValueAtDisposalUsd { get; set; }
        public decimal GainLossUsd { get; set; }
        public bool IsTaxable { get; set; }
        public required string TaxTreatment { get; set; } // Capital Gain, Income, etc.
        public required string TransactionId { get; set; }
        public required string Notes { get; set; }
    }

    public class TreasuryLimitsDto
    {
        public required string ClientId { get; set; }
        public Dictionary<string, decimal> DailyWithdrawalLimits { get; set; } = new(); // Asset → amount
        public Dictionary<string, decimal> MonthlyWithdrawalLimits { get; set; } = new();
        public decimal MaxSingleTransactionUsd { get; set; }
        public decimal CurrentDailyWithdrawalUsd { get; set; }
        public decimal CurrentMonthlyWithdrawalUsd { get; set; }
        public List<string> ApprovedWithdrawalAddresses { get; set; } = new();
        public int RequiredApprovalsForLargeTransactions { get; set; }
        public List<string> ApproverUserIds { get; set; } = new();
    }
}