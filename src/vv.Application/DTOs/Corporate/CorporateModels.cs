using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.Corporate
{
    public class CorporateClientDto
    {
        public string ClientId { get; set; }
        public string CompanyName { get; set; }
        public List<VaultPositionDto> VaultPositions { get; set; } = new();
        public Dictionary<string, decimal> TotalExposure { get; set; } = new(); // Asset → Amount
        public decimal TotalValueUsd { get; set; }
        public List<KYCStatusDto> KycStatus { get; set; } = new();
        public string RiskProfile { get; set; }
        public decimal WithdrawalLimit24h { get; set; }
        public string TreasuryIntegration { get; set; } // How this connects to their treasury
        public List<UserAccessDto> Administrators { get; set; } = new();
        public List<string> AuthorizedAddresses { get; set; } = new();
        public DateTime ClientSince { get; set; }
    }

    public class VaultPositionDto
    {
        public string PoolId { get; set; }
        public string PoolName { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal UnrealizedProfitLoss { get; set; }
        public decimal UnrealizedProfitLossPercent { get; set; }
        public decimal RealizedProfitLoss { get; set; }
        public Dictionary<string, decimal> AssetAllocation { get; set; } = new(); // Current allocation
        public DateTime EntryDate { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();
        public string Status { get; set; } // Active, Pending, Closed
        public DateTime LockupEnd { get; set; } // If applicable
        public decimal EstimatedYield { get; set; }
    }

    public class UserAccessDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime LastLogin { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }

    public class TransactionDto
    {
        public string TransactionId { get; set; }
        public string Type { get; set; } // Deposit, Withdrawal, Interest, Fee
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountUsd { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public string TxHash { get; set; } // If blockchain transaction
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public decimal Fee { get; set; }
    }

    public class CorporateSettingsDto
    {
        public string ClientId { get; set; }
        public Dictionary<string, bool> NotificationPreferences { get; set; } = new();
        public Dictionary<string, string> ApiIntegrations { get; set; } = new();
        public List<string> WhitelistedIps { get; set; } = new();
        public List<string> WhitelistedDomains { get; set; } = new();
        public bool RequireMultiSigForWithdrawals { get; set; }
        public int RequiredSignatures { get; set; }
        public decimal LargeTransactionThreshold { get; set; }
        public bool AutoReinvestment { get; set; }
        public string ReportingCurrency { get; set; }
        public string ReportingFrequency { get; set; }
        public List<string> ReportDeliveryEmails { get; set; } = new();
    }

    public class CorporateOnboardingDto
    {
        public string OnboardingId { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; } // InProgress, PendingDocuments, PendingApproval, Approved, Rejected
        public List<OnboardingStepDto> Steps { get; set; } = new();
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedToUserId { get; set; }
        public string RejectionReason { get; set; }
        public List<string> DocumentsRequired { get; set; } = new();
        public List<string> DocumentsSubmitted { get; set; } = new();
    }

    public class OnboardingStepDto
    {
        public required string StepId { get; set; }
        public required string Name { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; }
        public List<string> Notes { get; set; } = new();
    }
}